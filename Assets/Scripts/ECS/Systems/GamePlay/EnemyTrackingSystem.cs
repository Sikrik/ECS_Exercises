// 路径: Assets/Scripts/ECS/Systems/GamePlay/EnemyTrackingSystem.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人追踪系统 (Pro 版重构)
/// 职责：计算敌人的移动意图和攻击意图，并针对特定战局提供自适应 AI
/// </summary>
public class EnemyTrackingSystem : SystemBase
{
    #region 常量定义

    /// <summary>
    /// 最小距离阈值，用于避免浮点数精度问题导致的除零或无效计算
    /// </summary>
    private const float MinDistanceThreshold = 0.001f;

    /// <summary>
    /// 屏幕外敌人的更新间隔帧数，用于降低非可见敌人的 AI 计算频率以优化性能
    /// </summary>
    private const int OffScreenUpdateInterval = 15;

    /// <summary>
    /// 犹豫状态阈值，Perlin 噪声超过此值时敌人进入犹豫状态，移动幅度大幅降低
    /// </summary>
    private const float HesitationThreshold = 0.88f;

    /// <summary>
    /// 抖动权重系数，控制高频噪声对侧向移动的影响程度
    /// </summary>
    private const float JitterWeight = 0.4f;

    /// <summary>
    /// 远程敌人接近目标时的侧向移动权重
    /// </summary>
    private const float RangedSideStepWeightApproach = 0.3f;

    /// <summary>
    /// 远程敌人远离目标时的侧向移动权重
    /// </summary>
    private const float RangedSideStepWeightRetreat = 0.8f;

    /// <summary>
    /// 远程敌人在理想距离内徘徊时的侧向移动权重
    /// </summary>
    private const float RangedSideStepWeightIdle = 0.5f;

    /// <summary>
    /// 近战敌人的侧向移动权重，用于增加追击时的不可预测性
    /// </summary>
    private const float MeleeSideStepWeight = 0.4f;

    /// <summary>
    /// 犹豫状态下的移动缩放系数，使敌人移动速度显著降低
    /// </summary>
    private const float HesitationMovementScale = 0.2f;

    /// <summary>
    /// 群体分离半径的平方值，用于优化距离比较性能（避免开方运算）
    /// </summary>
    private const float SeparationRadiusSqr = 2.25f;

    /// <summary>
    /// 容忍度变化振幅，用于为每个敌人生成独特的行为模式
    /// </summary>
    private const float ToleranceVariationAmplitude = 0.5f;

    /// <summary>
    /// [Pro] 孤狼狂暴提速倍率，当仅剩一个屏幕外敌人时的紧急加速系数
    /// </summary>
    private const float EmergencySpeedMultiplier = 2.5f;

    #endregion

    #region 构造方法

    /// <summary>
    /// 初始化敌人追踪系统
    /// </summary>
    /// <param name="entities">系统管理的实体列表</param>
    public EnemyTrackingSystem(List<Entity> entities) : base(entities) { }

    #endregion

    #region 主循环

    /// <summary>
    /// 每帧更新敌人 AI 逻辑
    /// </summary>
    /// <param name="deltaTime">上一帧到当前帧的时间间隔（秒）</param>
    public override void Update(float deltaTime)
    {
        // 获取所有带有敌人标签和位置组件的实体
        var enemies = GetEntitiesWith<EnemyTag, PositionComponent>();
        var player = ECSManager.Instance.PlayerEntity;

        // 玩家不存在或已死亡时跳过处理
        if (player == null || !player.IsAlive) return;

        var playerPosition = player.GetComponent<PositionComponent>();
        var playerVelocity = player.GetComponent<VelocityComponent>();

        // 1. [Pro] 一次遍历，同时完成存活计数与实体捕获，极低开销
        int aliveCount = 0;
        Entity lastEnemy = null;
        for (int i = 0; i < enemies.Count; i++)
        {
            var e = enemies[i];
            if (e.IsAlive && !e.HasComponent<DeadTag>() && !e.HasComponent<PendingDestroyComponent>())
            {
                aliveCount++;
                lastEnemy = e;
            }
        }

        // 2. 状态断言：是否满足"全场仅剩一人且在屏幕外"
        bool isEmergencyMode = (aliveCount == 1 && lastEnemy != null && lastEnemy.HasComponent<OffScreenTag>());

        foreach (var enemy in enemies)
        {
            bool isThisEmergencyTarget = isEmergencyMode && enemy == lastEnemy;

            // 3. 传入上下文状态，决定是否执行 AI
            if (!ShouldProcessEnemy(enemy, isThisEmergencyTarget)) continue;

            // 4. 路由：紧急入场逻辑 vs 常规战术逻辑
            if (isThisEmergencyTarget)
            {
                ProcessEmergencyRecall(enemy, playerPosition);
            }
            else
            {
                ProcessEnemyAI(enemy, player, playerPosition, playerVelocity);
            }
        }
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 判断是否应该处理该敌人的 AI 逻辑
    /// </summary>
    /// <param name="enemy">待检测的敌人实体</param>
    /// <param name="isEmergencyTarget">是否为紧急召回目标（孤狼模式）</param>
    /// <returns>true 表示需要处理该敌人的 AI，false 表示跳过本帧更新</returns>
    private bool ShouldProcessEnemy(Entity enemy, bool isEmergencyTarget)
    {
        // 硬控与蓄力状态下，绝对停止思考 (不可覆盖)
        if (enemy.HasComponent<KnockbackComponent>() || enemy.HasComponent<HitRecoveryComponent>()) return false;
        if (enemy.HasComponent<ShootPrepStateComponent>() || enemy.HasComponent<DashPrepStateComponent>() || enemy.HasComponent<DashStateComponent>()) return false;

        // [Pro] 关键优化：如果是紧急召回目标，强行绕过屏幕外降频限制，赋予其100%帧率的丝滑转向能力
        if (!isEmergencyTarget && enemy.HasComponent<OffScreenTag>() && Time.frameCount % OffScreenUpdateInterval != 0)
            return false;

        return true;
    }

    /// <summary>
    /// [Pro] 孤狼紧急召回协议：剥离一切战术噪音，以绝对的最短路径全速突进
    /// 适用场景：全场仅剩一个敌人且该敌人在屏幕外时触发
    /// </summary>
    /// <param name="enemy">需要紧急召回的敌人实体</param>
    /// <param name="playerPosition">玩家当前位置组件</param>
    private void ProcessEmergencyRecall(Entity enemy, PositionComponent playerPosition)
    {
        var enemyPosition = enemy.GetComponent<PositionComponent>();
        Vector2 currentPosition = new Vector2(enemyPosition.X, enemyPosition.Y);
        Vector2 targetPosition = new Vector2(playerPosition.X, playerPosition.Y);
        
        Vector2 toTarget = targetPosition - currentPosition;
        
        if (toTarget.sqrMagnitude > MinDistanceThreshold)
        {
            // 1. 绝对意图：忽略 Perlin 噪音抖动、同类排斥、风筝距离等所有参数，笔直逼近摄像机中心(玩家)
            WriteMoveIntent(enemy, toTarget.normalized);

            // 2. 无副作用提速：巧妙利用 ECS 管线执行顺序。
            // 因为上一环节 StatusGatherSystem 已算出基础速度，此时我们覆写 CurrentSpeed，
            // 效果仅在本帧生效，且能立刻被下一环节的 MovementSystem 消费。
            // 一旦怪物踏入屏幕失去 OffScreenTag，提速效果会自动坍缩蒸发。
            var speed = enemy.GetComponent<SpeedComponent>();
            if (speed != null)
            {
                speed.CurrentSpeed *= EmergencySpeedMultiplier;
            }
        }
    }

    /// <summary>
    /// 处理常规敌人 AI 逻辑：计算目标位置、期望方向并应用群体分离
    /// </summary>
    /// <param name="enemy">待处理的敌人实体</param>
    /// <param name="player">玩家实体引用</param>
    /// <param name="playerPosition">玩家位置组件</param>
    /// <param name="playerVelocity">玩家速度组件（用于预测移动）</param>
    private void ProcessEnemyAI(Entity enemy, Entity player, PositionComponent playerPosition, VelocityComponent playerVelocity)
    {
        var enemyPosition = enemy.GetComponent<PositionComponent>();
        Vector2 currentPosition = new Vector2(enemyPosition.X, enemyPosition.Y);

        // 计算考虑预测的目标位置
        Vector2 targetPosition = CalculateTargetPosition(enemy, playerPosition, playerVelocity);
        Vector2 toTarget = targetPosition - currentPosition;
        float distanceToTarget = toTarget.magnitude;

        // 计算期望移动方向并应用群体分离
        Vector2 desiredDirection = CalculateDesiredDirection(enemy, currentPosition, toTarget, distanceToTarget);
        ApplySwarmSeparation(enemy, currentPosition, ref desiredDirection);
        WriteMoveIntent(enemy, desiredDirection);
    }

    /// <summary>
    /// 计算敌人的目标位置，支持预测性 AI
    /// </summary>
    /// <param name="enemy">敌人实体</param>
    /// <param name="playerPosition">玩家当前位置</param>
    /// <param name="playerVelocity">玩家当前速度</param>
    /// <returns>计算后的目标位置向量</returns>
    private Vector2 CalculateTargetPosition(Entity enemy, PositionComponent playerPosition, VelocityComponent playerVelocity)
    {
        Vector2 targetPosition = new Vector2(playerPosition.X, playerPosition.Y);

        // 如果敌人具有预测性 AI 组件且玩家速度有效，则进行移动预测
        if (enemy.HasComponent<PredictiveAIComponent>() && playerVelocity != null)
        {
            var predictiveAI = enemy.GetComponent<PredictiveAIComponent>();
            targetPosition.x += playerVelocity.VX * predictiveAI.PredictTime;
            targetPosition.y += playerVelocity.VY * predictiveAI.PredictTime;
        }
        return targetPosition;
    }

    /// <summary>
    /// 计算敌人的期望移动方向，综合考虑犹豫状态、侧向移动和敌人类型
    /// </summary>
    /// <param name="enemy">敌人实体</param>
    /// <param name="currentPosition">敌人当前位置</param>
    /// <param name="toTarget">从敌人指向目标的向量</param>
    /// <param name="distanceToTarget">敌人与目标的距离</param>
    /// <returns>归一化后的期望移动方向向量</returns>
    private Vector2 CalculateDesiredDirection(Entity enemy, Vector2 currentPosition, Vector2 toTarget, float distanceToTarget)
    {
        float currentTime = Time.time;
        int entityHash = enemy.GetHashCode();

        // 计算侧向移动方向（用于增加 AI 行为的随机性和自然感）
        Vector2 sideStepDirection = CalculateSideStepDirection(toTarget, distanceToTarget, entityHash, currentTime);
        
        // 基于 Perlin 噪声判断是否进入犹豫状态
        bool isHesitating = Mathf.PerlinNoise(entityHash * 0.05f, currentTime * 0.2f) > HesitationThreshold;

        // 犹豫状态下大幅降低移动幅度
        if (isHesitating) return sideStepDirection * HesitationMovementScale;

        // 根据敌人类型选择不同的移动策略
        if (enemy.HasComponent<RangedAIComponent>())
        {
            return CalculateRangedMovementDirection(enemy, toTarget, distanceToTarget, sideStepDirection, entityHash, currentTime);
        }
        else
        {
            // 近战敌人：直接追击并附加少量侧向移动
            if (distanceToTarget > MinDistanceThreshold)
            {
                return (toTarget.normalized + sideStepDirection * MeleeSideStepWeight).normalized;
            }
        }
        return Vector2.zero;
    }

    /// <summary>
    /// 计算侧向移动方向，使用双层 Perlin 噪声组合实现自然的随机摆动
    /// </summary>
    /// <param name="toTarget">从敌人指向目标的向量</param>
    /// <param name="distanceToTarget">敌人与目标的距离</param>
    /// <param name="entityHash">实体哈希值，用于为每个敌人生成独立的噪声序列</param>
    /// <param name="currentTime">当前游戏时间</param>
    /// <returns>侧向移动方向向量（垂直于目标方向）</returns>
    private Vector2 CalculateSideStepDirection(Vector2 toTarget, float distanceToTarget, int entityHash, float currentTime)
    {
        // 距离过近时不进行侧向移动
        if (distanceToTarget <= MinDistanceThreshold) return Vector2.zero;

        // 低频基础噪声：产生缓慢的方向变化
        float baseNoise = Mathf.PerlinNoise(entityHash * 0.1f, currentTime * 0.5f) * 2f - 1f;
        
        // 高频抖动噪声：增加快速的不规则扰动
        float jitterNoise = Mathf.PerlinNoise(entityHash * 0.8f, currentTime * 2.5f) * 2f - 1f;
        
        // 组合两种噪声并限制在 [-1, 1] 范围内
        float combinedNoise = Mathf.Clamp(baseNoise + jitterNoise * JitterWeight, -1f, 1f);

        // 计算垂直于目标方向的向量并应用噪声权重
        Vector2 perpendicularDirection = new Vector2(-toTarget.y, toTarget.x).normalized;
        return perpendicularDirection * combinedNoise;
    }

    /// <summary>
    /// 计算远程敌人的移动方向，根据与理想距离的关系选择接近、远离或徘徊策略
    /// </summary>
    /// <param name="enemy">远程敌人实体</param>
    /// <param name="toTarget">从敌人指向目标的向量</param>
    /// <param name="distanceToTarget">敌人与目标的实际距离</param>
    /// <param name="sideStepDirection">预计算的侧向移动方向</param>
    /// <param name="entityHash">实体哈希值</param>
    /// <param name="currentTime">当前游戏时间</param>
    /// <returns>归一化后的移动方向向量</returns>
    private Vector2 CalculateRangedMovementDirection(Entity enemy, Vector2 toTarget, float distanceToTarget, 
        Vector2 sideStepDirection, int entityHash, float currentTime)
    {
        var rangedAI = enemy.GetComponent<RangedAIComponent>();
        
        // 为每个敌人生成独特的容忍度变化曲线，避免所有远程敌人同步行动
        float personalTolerance = rangedAI.Tolerance + (Mathf.Sin(currentTime + entityHash) * ToleranceVariationAmplitude);

        // 距离过远：接近目标
        if (distanceToTarget > rangedAI.PreferredDistance + personalTolerance)
        {
            return (toTarget.normalized + sideStepDirection * RangedSideStepWeightApproach).normalized;
        }
        // 距离过近：远离目标
        else if (distanceToTarget < rangedAI.PreferredDistance - personalTolerance)
        {
            return (-toTarget.normalized + sideStepDirection * RangedSideStepWeightRetreat).normalized;
        }
        // 在理想距离内：横向徘徊
        else
        {
            return sideStepDirection * RangedSideStepWeightIdle;
        }
    }

    /// <summary>
    /// 应用群体分离算法，避免敌人之间过度拥挤
    /// 使用空间网格查询优化邻近敌人检索性能
    /// </summary>
    /// <param name="enemy">当前处理的敌人实体</param>
    /// <param name="currentPosition">当前敌人的位置</param>
    /// <param name="desiredDirection">当前的期望移动方向（引用传递，可能被修改）</param>
    private void ApplySwarmSeparation(Entity enemy, Vector2 currentPosition, ref Vector2 desiredDirection)
    {
        // 没有群体分离组件则跳过
        if (!enemy.HasComponent<SwarmSeparationComponent>()) return;

        var swarmSeparation = enemy.GetComponent<SwarmSeparationComponent>();
        Vector2 avoidanceDirection = Vector2.zero;
        
        // 通过空间网格快速获取附近的敌人（半径为 1 单位）
        var nearbyEnemies = ECSManager.Instance.Grid.GetNearbyEnemies(currentPosition.x, currentPosition.y, 1);

        foreach (var otherEnemy in nearbyEnemies)
        {
            // 跳过自身和非敌人实体
            if (otherEnemy == enemy || !otherEnemy.HasComponent<EnemyTag>()) continue;

            var otherPosition = otherEnemy.GetComponent<PositionComponent>();
            Vector2 positionDifference = currentPosition - new Vector2(otherPosition.X, otherPosition.Y);
            float squaredDistance = positionDifference.sqrMagnitude;

            // 在分离半径内且距离不为零时累加排斥力（反比于距离平方）
            if (squaredDistance < SeparationRadiusSqr && squaredDistance > MinDistanceThreshold)
            {
                avoidanceDirection += positionDifference.normalized / squaredDistance;
            }
        }

        // 将排斥力整合到期望方向中
        if (avoidanceDirection != Vector2.zero)
        {
            // 如果原本没有移动意图，直接使用排斥方向
            if (desiredDirection == Vector2.zero)
            {
                desiredDirection = avoidanceDirection.normalized;
            }
            // 否则将排斥力加权后与原方向混合
            else
            {
                desiredDirection = (desiredDirection + avoidanceDirection * swarmSeparation.SeparationWeight).normalized;
            }
        }
    }

    /// <summary>
    /// 写入移动意图到实体的 MoveInputComponent
    /// 如果组件不存在则创建，存在则更新
    /// </summary>
    /// <param name="enemy">目标敌人实体</param>
    /// <param name="desiredDirection">计算得到的期望移动方向</param>
    private void WriteMoveIntent(Entity enemy, Vector2 desiredDirection)
    {
        if (!enemy.HasComponent<MoveInputComponent>())
        {
            enemy.AddComponent(new MoveInputComponent(desiredDirection.x, desiredDirection.y));
        }
        else
        {
            var moveInput = enemy.GetComponent<MoveInputComponent>();
            moveInput.X = desiredDirection.x;
            moveInput.Y = desiredDirection.y;
        }
    }

    #endregion
}
