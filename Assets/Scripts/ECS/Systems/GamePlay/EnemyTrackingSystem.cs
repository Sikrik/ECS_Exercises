// 路径: Assets/Scripts/ECS/Systems/GamePlay/EnemyTrackingSystem.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人追踪系统 (Pro 版重构)
/// 职责：计算敌人的移动意图和攻击意图，并针对特定战局提供自适应 AI
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
    
    // [Pro] 孤狼狂暴提速倍率
    private const float EMERGENCY_SPEED_MULTIPLIER = 2.5f; 

    public EnemyTrackingSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var enemies = GetEntitiesWith<EnemyTag, PositionComponent>();
        var player = ECSManager.Instance.PlayerEntity;

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

        // 2. 状态断言：是否满足“全场仅剩一人且在屏幕外”
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

    /// <summary>
    /// 判断是否应该处理该敌人的 AI 逻辑 (重构版)
    /// </summary>
    private bool ShouldProcessEnemy(Entity enemy, bool isEmergencyTarget)
    {
        // 硬控与蓄力状态下，绝对停止思考 (不可覆盖)
        if (enemy.HasComponent<KnockbackComponent>() || enemy.HasComponent<HitRecoveryComponent>()) return false;
        if (enemy.HasComponent<ShootPrepStateComponent>() || enemy.HasComponent<DashPrepStateComponent>() || enemy.HasComponent<DashStateComponent>()) return false;

        // [Pro] 关键优化：如果是紧急召回目标，强行绕过屏幕外降频限制，赋予其100%帧率的丝滑转向能力
        if (!isEmergencyTarget && enemy.HasComponent<OffScreenTag>() && Time.frameCount % OFF_SCREEN_UPDATE_INTERVAL != 0)
            return false;

        return true;
    }

    /// <summary>
    /// [Pro] 孤狼紧急召回协议：剥离一切战术噪音，以绝对的最短路径全速突进
    /// </summary>
    private void ProcessEmergencyRecall(Entity enemy, PositionComponent playerPosition)
    {
        var enemyPosition = enemy.GetComponent<PositionComponent>();
        Vector2 currentPosition = new Vector2(enemyPosition.X, enemyPosition.Y);
        Vector2 targetPosition = new Vector2(playerPosition.X, playerPosition.Y);
        
        Vector2 toTarget = targetPosition - currentPosition;
        
        if (toTarget.sqrMagnitude > MIN_DISTANCE_THRESHOLD)
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
                speed.CurrentSpeed *= EMERGENCY_SPEED_MULTIPLIER;
            }
        }
    }

    // ==========================================
    // 下方的常规 AI 战术算法保持完全不变
    // ==========================================
    private void ProcessEnemyAI(Entity enemy, Entity player, PositionComponent playerPosition, VelocityComponent playerVelocity)
    {
        var enemyPosition = enemy.GetComponent<PositionComponent>();
        Vector2 currentPosition = new Vector2(enemyPosition.X, enemyPosition.Y);

        Vector2 targetPosition = CalculateTargetPosition(enemy, playerPosition, playerVelocity);
        Vector2 toTarget = targetPosition - currentPosition;
        float distanceToTarget = toTarget.magnitude;

        Vector2 desiredDirection = CalculateDesiredDirection(enemy, currentPosition, toTarget, distanceToTarget);
        ApplySwarmSeparation(enemy, currentPosition, ref desiredDirection);
        WriteMoveIntent(enemy, desiredDirection);
    }

    private Vector2 CalculateTargetPosition(Entity enemy, PositionComponent playerPosition, VelocityComponent playerVelocity)
    {
        Vector2 targetPosition = new Vector2(playerPosition.X, playerPosition.Y);

        if (enemy.HasComponent<PredictiveAIComponent>() && playerVelocity != null)
        {
            var predictiveAI = enemy.GetComponent<PredictiveAIComponent>();
            targetPosition.x += playerVelocity.VX * predictiveAI.PredictTime;
            targetPosition.y += playerVelocity.VY * predictiveAI.PredictTime;
        }
        return targetPosition;
    }

    private Vector2 CalculateDesiredDirection(Entity enemy, Vector2 currentPosition, Vector2 toTarget, float distanceToTarget)
    {
        float currentTime = Time.time;
        int entityHash = enemy.GetHashCode();

        Vector2 sideStepDirection = CalculateSideStepDirection(toTarget, distanceToTarget, entityHash, currentTime);
        bool isHesitating = Mathf.PerlinNoise(entityHash * 0.05f, currentTime * 0.2f) > HESITATION_THRESHOLD;

        if (isHesitating) return sideStepDirection * HESITATION_MOVEMENT_SCALE;

        if (enemy.HasComponent<RangedAIComponent>())
        {
            return CalculateRangedMovementDirection(enemy, toTarget, distanceToTarget, sideStepDirection, entityHash, currentTime);
        }
        else
        {
            if (distanceToTarget > MIN_DISTANCE_THRESHOLD)
            {
                return (toTarget.normalized + sideStepDirection * MELEE_SIDE_STEP_WEIGHT).normalized;
            }
        }
        return Vector2.zero;
    }

    private Vector2 CalculateSideStepDirection(Vector2 toTarget, float distanceToTarget, int entityHash, float currentTime)
    {
        if (distanceToTarget <= MIN_DISTANCE_THRESHOLD) return Vector2.zero;

        float baseNoise = Mathf.PerlinNoise(entityHash * 0.1f, currentTime * 0.5f) * 2f - 1f;
        float jitterNoise = Mathf.PerlinNoise(entityHash * 0.8f, currentTime * 2.5f) * 2f - 1f;
        float combinedNoise = Mathf.Clamp(baseNoise + jitterNoise * JITTER_WEIGHT, -1f, 1f);

        Vector2 perpendicularDirection = new Vector2(-toTarget.y, toTarget.x).normalized;
        return perpendicularDirection * combinedNoise;
    }

    private Vector2 CalculateRangedMovementDirection(Entity enemy, Vector2 toTarget, float distanceToTarget, 
        Vector2 sideStepDirection, int entityHash, float currentTime)
    {
        var rangedAI = enemy.GetComponent<RangedAIComponent>();
        float personalTolerance = rangedAI.Tolerance + (Mathf.Sin(currentTime + entityHash) * TOLERANCE_VARIATION_AMPLITUDE);

        if (distanceToTarget > rangedAI.PreferredDistance + personalTolerance)
        {
            return (toTarget.normalized + sideStepDirection * RANGED_SIDE_STEP_WEIGHT_APPROACH).normalized;
        }
        else if (distanceToTarget < rangedAI.PreferredDistance - personalTolerance)
        {
            return (-toTarget.normalized + sideStepDirection * RANGED_SIDE_STEP_WEIGHT_RETREAT).normalized;
        }
        else
        {
            return sideStepDirection * RANGED_SIDE_STEP_WEIGHT_IDLE;
        }
    }

    private void ApplySwarmSeparation(Entity enemy, Vector2 currentPosition, ref Vector2 desiredDirection)
    {
        if (!enemy.HasComponent<SwarmSeparationComponent>()) return;

        var swarmSeparation = enemy.GetComponent<SwarmSeparationComponent>();
        Vector2 avoidanceDirection = Vector2.zero;
        var nearbyEnemies = ECSManager.Instance.Grid.GetNearbyEnemies(currentPosition.x, currentPosition.y, 1);

        foreach (var otherEnemy in nearbyEnemies)
        {
            if (otherEnemy == enemy || !otherEnemy.HasComponent<EnemyTag>()) continue;

            var otherPosition = otherEnemy.GetComponent<PositionComponent>();
            Vector2 positionDifference = currentPosition - new Vector2(otherPosition.X, otherPosition.Y);
            float squaredDistance = positionDifference.sqrMagnitude;

            if (squaredDistance < SEPARATION_RADIUS_SQR && squaredDistance > MIN_DISTANCE_THRESHOLD)
            {
                avoidanceDirection += positionDifference.normalized / squaredDistance;
            }
        }

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