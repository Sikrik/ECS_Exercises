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
    private bool ShouldProcessEnemy(Entity enemy)
    {
        // 硬控状态下停止思考
        if (enemy.HasComponent<KnockbackComponent>() || enemy.HasComponent<HitRecoveryComponent>())
            return false;

        // 👇 【核心修复】：蓄力、瞄准、冲刺状态下，必须彻底停止寻路思考，防止在开火/冲锋前发生诡异的漂移
        if (enemy.HasComponent<ShootPrepStateComponent>() || 
            enemy.HasComponent<DashPrepStateComponent>() || 
            enemy.HasComponent<DashStateComponent>())
            return false;

        // 降频优化：屏幕外降低寻路计算频率
        if (enemy.HasComponent<OffScreenTag>() && Time.frameCount % OFF_SCREEN_UPDATE_INTERVAL != 0)
            return false;

        return true;
    }

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

        if (isHesitating)
        {
            return sideStepDirection * HESITATION_MOVEMENT_SCALE;
        }

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