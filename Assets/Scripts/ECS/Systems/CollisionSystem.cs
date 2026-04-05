using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 碰撞系统：利用 Unity 物理引擎获取法线，实现高精度反弹逻辑
/// </summary>
public class CollisionSystem : SystemBase
{
    private Collider2D[] _results = new Collider2D[20];
    private ContactFilter2D _enemyFilter;

    public CollisionSystem(List<Entity> entities) : base(entities)
    {
        // 仅检测 Enemy 层级的物体
        _enemyFilter = new ContactFilter2D();
        _enemyFilter.SetLayerMask(LayerMask.GetMask("Enemy"));
        _enemyFilter.useTriggers = true;
    }

    public override void Update(float deltaTime)
    {
        var ecs = ECSManager.Instance;
        var player = ecs.PlayerEntity;
        if (player == null || !player.IsAlive) return;

        // 获取由 PhysicsBakingSystem 自动烘焙的物理组件
        var pPhys = player.GetComponent<PhysicsColliderComponent>();
        if (pPhys == null || pPhys.Collider == null) return;

        // --- 1. 空间查询 ---
        int hitCount = pPhys.Collider.OverlapCollider(_enemyFilter, _results);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D enemyCollider = _results[i];
            Entity enemy = ecs.GetEntityFromGameObject(enemyCollider.gameObject);
            
            if (enemy != null && enemy.IsAlive && enemy.HasComponent<EnemyTag>())
            {
                HandlePhysicsBounce(player, enemy, pPhys.Collider, enemyCollider);
            }
        }
    }

    private void HandlePhysicsBounce(Entity player, Entity enemy, Collider2D pCol, Collider2D eCol)
    {
        var config = ECSManager.Instance.Config;
        
        // --- 2. 法线计算：利用 Distance 获取垂直于切线的方向 ---
        ColliderDistance2D distInfo = pCol.Distance(eCol);

        if (distInfo.isOverlapped)
        {
            Vector2 normal = distInfo.normal; // 法线方向

            // 3. 执行反弹逻辑
            if (enemy.HasComponent<BouncyTag>())
            {
                var ePos = enemy.GetComponent<PositionComponent>();
                var eVel = enemy.GetComponent<VelocityComponent>();

                // 应用 Config 中的反弹参数
                float pushDistance = config.CollisionPushDistance;
                float bounceForce = config.CollisionBounceForce;

                // 修正坐标防止嵌入
                ePos.X += normal.x * pushDistance;
                ePos.Y += normal.y * pushDistance;

                // 赋予反弹初速度
                if (eVel != null)
                {
                    eVel.VX = normal.x * bounceForce;
                    eVel.VY = normal.y * bounceForce;
                }
                
                // 标记进入受击硬直
                enemy.AddComponent(new HitRecoveryComponent { Timer = config.EnemyHitRecoveryDuration });
            }

            // 4. 处理玩家受损
            ApplyDamage(player, enemy, config);
        }
    }

    private void ApplyDamage(Entity player, Entity enemy, GameConfig config)
    {
        if (!player.HasComponent<InvincibleComponent>())
        {
            var pHealth = player.GetComponent<HealthComponent>();
            var eStats = enemy.GetComponent<EnemyStatsComponent>();
            
            pHealth.CurrentHealth -= (eStats != null ? eStats.Damage : 10);
            
            // 触发受击无敌闪烁
            player.AddComponent(new InvincibleComponent { RemainingTime = config.PlayerInvincibleDuration });
            Debug.Log($"玩家受撞击！剩余血量: {pHealth.CurrentHealth}");
        }
    }
}