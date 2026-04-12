// 路径: Assets/Scripts/ECS/Systems/GamePlay/EnemyTrackingSystem.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人追踪系统
/// 职责：计算敌人的移动意图和攻击意图，实现智能 AI 行为（追踪、预判、风筝、包围等）
/// </summary>
public class EnemyTrackingSystem : SystemBase
{
    private const float MIN_DISTANCE_THRESHOLD = 0.001f;
    private const int OFF_SCREEN_UPDATE_INTERVAL = 15;
    private const float HESITATION_THRESHOLD = 0.88f;
    private const float JITTER_WEIGHT = 0.4f;
    private const float RANGED_SIDE_STEP_WEIGHT_APPROACH = 0.3f;
    private const float RANGED_SIDE_STEP_WEIGHT_RETREAT = 0.8f;
    private const float RANGED_SIDE_STEP_WEIGHT_IDLE = 0.5f;
    private const float MELEE_SIDE_STEP_WEIGHT = 0.4f;
    private const float HESITATION_MOVEMENT_SCALE = 0.2f;
    private const float SEPARATION_RADIUS_SQR = 2.25f;
    private const float TOLERANCE_VARIATION_AMPLITUDE = 0.5f;
    private const float FIRE_RANGE_BUFFER = 1f;

    public EnemyTrackingSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var enemies = GetEntitiesWith<EnemyTag, PositionComponent>();
        var player = ECSManager.Instance.PlayerEntity;

        if (player == null || !player.IsAlive) return;

        var playerPosition = player.GetComponent<PositionComponent>();
        var playerVelocity = player.GetComponent<VelocityComponent>();

        foreach (var enemy in enemies)
        {
            if (!ShouldProcessEnemy(enemy)) continue;

            ProcessEnemyAI(enemy, player, playerPosition, playerVelocity);
        }


    }

    /// <summary>
    /// 判断是否应该处理该敌人的 AI 逻辑
    /// </summary>
    /// <param name="enemy">敌人实体</param>
    /// <returns>是否应该处理</returns>
    private bool ShouldProcessEnemy(Entity enemy)
    {
        // 硬控状态下停止思考
        if (enemy.HasComponent<KnockbackComponent>() || enemy.HasComponent<HitRecoveryComponent>())
            return false;

        // 降频优化：屏幕外降低寻路计算频率
        if (enemy.HasComponent<OffScreenTag>() && Time.frameCount % OFF_SCREEN_UPDATE_INTERVAL != 0)
            return false;

        return true;
    }

    /// <summary>
    /// 处理单个敌人的 AI 逻辑
    /// </summary>
    /// <param name="enemy">敌人实体</param>
    /// <param name="player">玩家实体</param>
    /// <param name="playerPosition">玩家位置组件</param>
    /// <param name="playerVelocity">玩家速度组件</param>
    private void ProcessEnemyAI(Entity enemy, Entity player, PositionComponent playerPosition, VelocityComponent playerVelocity)
    {
        var enemyPosition = enemy.GetComponent<PositionComponent>();
        Vector2 currentPosition = new Vector2(enemyPosition.X, enemyPosition.Y);

        // 确定目标点（基础追踪 vs 预判追踪）
        Vector2 targetPosition = CalculateTargetPosition(enemy, playerPosition, playerVelocity);
        Vector2 toTarget = targetPosition - currentPosition;
        float distanceToTarget = toTarget.magnitude;

        // 计算期望的移动方向
        Vector2 desiredDirection = CalculateDesiredDirection(enemy, currentPosition, toTarget, distanceToTarget);

        // 应用虫群分离逻辑（形成包围网）
        ApplySwarmSeparation(enemy, currentPosition, ref desiredDirection);

        // 写入移动意图组件（交由 MovementSystem 处理惯性和实际位移）
        WriteMoveIntent(enemy, desiredDirection);
    }

    /// <summary>
    /// 计算目标位置（支持预判追踪）
    /// </summary>
    /// <param name="enemy">敌人实体</param>
    /// <param name="playerPosition">玩家位置组件</param>
    /// <param name="playerVelocity">玩家速度组件</param>
    /// <returns>目标位置坐标</returns>
    private Vector2 CalculateTargetPosition(Entity enemy, PositionComponent playerPosition, VelocityComponent playerVelocity)
    {
        Vector2 targetPosition = new Vector2(playerPosition.X, playerPosition.Y);

        // 预判型 AI：预测玩家未来的位置
        if (enemy.HasComponent<PredictiveAIComponent>() && playerVelocity != null)
        {
            var predictiveAI = enemy.GetComponent<PredictiveAIComponent>();
            // 预判公式：目标位置 = 玩家当前位置 + 玩家速度 × 预判时间
            targetPosition.x += playerVelocity.VX * predictiveAI.PredictTime;
            targetPosition.y += playerVelocity.VY * predictiveAI.PredictTime;
        }

        return targetPosition;
    }

    /// <summary>
    /// 计算期望的移动方向（包含随机扰动和顿挫效果）
    /// </summary>
    /// <param name="enemy">敌人实体</param>
    /// <param name="currentPosition">当前位置</param>
    /// <param name="toTarget">指向目标的向量</param>
    /// <param name="distanceToTarget">到目标的距离</param>
    /// <returns>归一化的期望移动方向</returns>
    private Vector2 CalculateDesiredDirection(Entity enemy, Vector2 currentPosition, Vector2 toTarget, float distanceToTarget)
    {
        float currentTime = Time.time;
        int entityHash = enemy.GetHashCode();

        // 计算侧向扰动方向
        Vector2 sideStepDirection = CalculateSideStepDirection(toTarget, distanceToTarget, entityHash, currentTime);

        // 判断是否处于顿挫状态（模拟怪物偶尔"走神"或"停顿观察"）
        bool isHesitating = Mathf.PerlinNoise(entityHash * 0.05f, currentTime * 0.2f) > HESITATION_THRESHOLD;

        if (isHesitating)
        {
            // 顿挫状态：原地徘徊或小幅度侧移，增加攻击节奏的不可预测性
            return sideStepDirection * HESITATION_MOVEMENT_SCALE;
        }

        // 根据敌人类型计算不同的移动策略
        if (enemy.HasComponent<RangedAIComponent>())
        {
            return CalculateRangedMovementDirection(enemy, toTarget, distanceToTarget, sideStepDirection, entityHash, currentTime);
        }
        else
        {
            // 近战怪物：直接冲锋，混入曲线扰动使走位更灵动
            if (distanceToTarget > MIN_DISTANCE_THRESHOLD)
            {
                return (toTarget.normalized + sideStepDirection * MELEE_SIDE_STEP_WEIGHT).normalized;
            }
        }

        return Vector2.zero;
    }

    /// <summary>
    /// 计算侧向扰动方向（基于柏林噪声）
    /// </summary>
    /// <param name="toTarget">指向目标的向量</param>
    /// <param name="distanceToTarget">到目标的距离</param>
    /// <param name="entityHash">实体哈希值（用于个体差异化）</param>
    /// <param name="currentTime">当前游戏时间</param>
    /// <returns>侧向扰动方向向量</returns>
    private Vector2 CalculateSideStepDirection(Vector2 toTarget, float distanceToTarget, int entityHash, float currentTime)
    {
        if (distanceToTarget <= MIN_DISTANCE_THRESHOLD)
            return Vector2.zero;

        // 多重柏林噪声：基础平滑侧移 + 高频微小抖动
        float baseNoise = Mathf.PerlinNoise(entityHash * 0.1f, currentTime * 0.5f) * 2f - 1f;
        float jitterNoise = Mathf.PerlinNoise(entityHash * 0.8f, currentTime * 2.5f) * 2f - 1f;
        float combinedNoise = Mathf.Clamp(baseNoise + jitterNoise * JITTER_WEIGHT, -1f, 1f);

        // 计算垂直于目标方向的侧向向量
        Vector2 perpendicularDirection = new Vector2(-toTarget.y, toTarget.x).normalized;
        return perpendicularDirection * combinedNoise;
    }

    /// <summary>
    /// 计算远程怪物的移动方向（风筝战术）
    /// </summary>
    /// <param name="enemy">敌人实体</param>
    /// <param name="toTarget">指向玩家的向量</param>
    /// <param name="distanceToTarget">到玩家的距离</param>
    /// <param name="sideStepDirection">侧向扰动方向</param>
    /// <param name="entityHash">实体哈希值</param>
    /// <param name="currentTime">当前游戏时间</param>
    /// <returns>期望的移动方向</returns>
    private Vector2 CalculateRangedMovementDirection(Entity enemy, Vector2 toTarget, float distanceToTarget, 
        Vector2 sideStepDirection, int entityHash, float currentTime)
    {
        var rangedAI = enemy.GetComponent<RangedAIComponent>();

        // 动态容差：让不同个体的"撤退/前进"阈值产生差异，避免群体怪物动作过于一致
        float personalTolerance = rangedAI.Tolerance + (Mathf.Sin(currentTime + entityHash) * TOLERANCE_VARIATION_AMPLITUDE);

        // 距离太远，靠近玩家
        if (distanceToTarget > rangedAI.PreferredDistance + personalTolerance)
        {
            return (toTarget.normalized + sideStepDirection * RANGED_SIDE_STEP_WEIGHT_APPROACH).normalized;
        }
        // 距离太近，后退（风筝战术），并加入较强的侧向平移，避免死板倒退
        else if (distanceToTarget < rangedAI.PreferredDistance - personalTolerance)
        {
            return (-toTarget.normalized + sideStepDirection * RANGED_SIDE_STEP_WEIGHT_RETREAT).normalized;
        }
        // 距离合适，徘徊状态（小幅度左右横移寻找输出位置）
        else
        {
            return sideStepDirection * RANGED_SIDE_STEP_WEIGHT_IDLE;
        }
    }

    /// <summary>
    /// 应用虫群分离逻辑（形成包围网，避免怪物重叠）
    /// </summary>
    /// <param name="enemy">敌人实体</param>
    /// <param name="currentPosition">当前位置</param>
    /// <param name="desiredDirection">当前的期望移动方向（引用传递，会被修改）</param>
    private void ApplySwarmSeparation(Entity enemy, Vector2 currentPosition, ref Vector2 desiredDirection)
    {
        if (!enemy.HasComponent<SwarmSeparationComponent>())
            return;

        var swarmSeparation = enemy.GetComponent<SwarmSeparationComponent>();
        Vector2 avoidanceDirection = Vector2.zero;

        // 借用 GridSystem 高效获取周围的怪物
        var nearbyEnemies = ECSManager.Instance.Grid.GetNearbyEnemies(currentPosition.x, currentPosition.y, 1);

        foreach (var otherEnemy in nearbyEnemies)
        {
            if (otherEnemy == enemy || !otherEnemy.HasComponent<EnemyTag>())
                continue;

            var otherPosition = otherEnemy.GetComponent<PositionComponent>();
            Vector2 positionDifference = currentPosition - new Vector2(otherPosition.X, otherPosition.Y);
            float squaredDistance = positionDifference.sqrMagnitude;

            // 如果两个怪物太近（相距 1.5 米以内，即平方距离 < 2.25）
            if (squaredDistance < SEPARATION_RADIUS_SQR && squaredDistance > MIN_DISTANCE_THRESHOLD)
            {
                // 距离越近，排斥力越大（反比关系）
                avoidanceDirection += positionDifference.normalized / squaredDistance;
            }
        }

        // 将分离向量与目标移动向量融合
        if (avoidanceDirection != Vector2.zero)
        {
            if (desiredDirection == Vector2.zero)
            {
                desiredDirection = avoidanceDirection.normalized;
            }
            else
            {
                desiredDirection = (desiredDirection + avoidanceDirection * swarmSeparation.SeparationWeight).normalized;
            }
        }
    }

    /// <summary>
    /// 生成开火意图（远程怪物在射程内时）
    /// </summary>
    /// <param name="enemy">敌人实体</param>
    /// <param name="enemyPosition">敌人位置组件</param>
    /// <param name="playerPosition">玩家位置组件</param>
    /// <param name="distanceToTarget">到玩家的距离</param>
    /// <param name="rangedAI">远程 AI 组件</param>
    private void GenerateFireIntent(Entity enemy, PositionComponent enemyPosition, PositionComponent playerPosition, 
        float distanceToTarget, RangedAIComponent rangedAI)
    {
        // 只要在射程内，就产生开火意图（具体开火频率由 WeaponFiringSystem 的 CD 控制）
        if (distanceToTarget > rangedAI.PreferredDistance + rangedAI.Tolerance + FIRE_RANGE_BUFFER)
            return;

        if (enemy.HasComponent<FireIntentComponent>())
            return;

        // 射击方向永远是真实的玩家方向，不受预判和移动扰动的影响
        Vector2 trueToPlayer = new Vector2(playerPosition.X - enemyPosition.X, playerPosition.Y - enemyPosition.Y);
        
        if (trueToPlayer.magnitude <= MIN_DISTANCE_THRESHOLD)
            return;

        enemy.AddComponent(new FireIntentComponent(trueToPlayer.normalized));
    }

    /// <summary>
    /// 写入移动意图组件（交由 MovementSystem 处理惯性和实际位移）
    /// </summary>
    /// <param name="enemy">敌人实体</param>
    /// <param name="desiredDirection">期望的移动方向</param>
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
}
