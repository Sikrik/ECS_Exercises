using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 碰撞系统：利用 Unity 物理引擎获取法线，实现高精度反弹逻辑
/// </summary>
public class CollisionSystem : SystemBase
{
    // 预分配数组减少 GC 内存抖动
    private Collider2D[] _results = new Collider2D[20];
    private ContactFilter2D _enemyFilter;

    public CollisionSystem(List<Entity> entities) : base(entities)
    {
        // 设置过滤器：只检测在 "Enemy" 层级的物体
        // 请确保在 Unity 编辑器里创建了名为 "Enemy" 的 Layer
        _enemyFilter = new ContactFilter2D();
        _enemyFilter.SetLayerMask(LayerMask.GetMask("Enemy"));
        _enemyFilter.useTriggers = true; // 我们使用的是 Trigger 模式
    }

    public override void Update(float deltaTime)
    {
        var ecs = ECSManager.Instance;
        var player = ecs.PlayerEntity;
        if (player == null || !player.IsAlive) return;

        // 获取玩家的物理组件（由 PhysicsBakingSystem 自动烘焙）
        var pPhys = player.GetComponent<PhysicsColliderComponent>();
        if (pPhys == null || pPhys.Collider == null) return;

        // --- 第一步：空间查询 (Spatial Query) ---
        // 直接问 Unity：现在玩家的 Collider 撞到了哪些敌人？
        int hitCount = pPhys.Collider.OverlapCollider(_enemyFilter, _results);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D enemyCollider = _results[i];
            
            // 通过映射字典瞬间找回对应的 Entity
            Entity enemy = ecs.GetEntityFromGameObject(enemyCollider.gameObject);
            
            if (enemy != null && enemy.IsAlive && enemy.HasComponent<EnemyTag>())
            {
                HandlePhysicsBounce(player, enemy, pPhys.Collider, enemyCollider);
            }
        }
    }

    /// <summary>
    /// 处理基于法线的物理反弹
    /// </summary>
    private void HandlePhysicsBounce(Entity player, Entity enemy, Collider2D pCol, Collider2D eCol)
    {
        var config = ECSManager.Instance.Config;
        
        // --- 第二步：法线计算 (Normal Calculation) ---
        // Unity 的 Distance 方法会返回两个任意形状 Collider 之间的最短距离信息
        ColliderDistance2D distInfo = pCol.Distance(eCol);

        if (distInfo.isOverlapped)
        {
            // distInfo.normal 是垂直于接触面切线的向量（法线）
            // 它指向让两个物体分开的方向
            Vector2 normal = distInfo.normal;

            // 1. 处理反弹位移 (仅针对拥有 BouncyTag 的敌人)
            if (enemy.HasComponent<BouncyTag>())
            {
                var ePos = enemy.GetComponent<PositionComponent>();
                
                // 沿着法线方向推开一小段距离，解决“嵌入”问题
                // 这里的 0.2f 可以根据打击感需求调整
                float pushDistance = 0.2f; 
                ePos.X += normal.x * pushDistance;
                ePos.Y += normal.y * pushDistance;

                // 2. 处理反弹速度（可选：让敌人有个向后飞的速度）
                var eVel = enemy.GetComponent<VelocityComponent>();
                if (eVel != null)
                {
                    float bounceForce = 5.0f;
                    eVel.VX = normal.x * bounceForce;
                    eVel.VY = normal.y * bounceForce;
                }
            }

            // 3. 处理伤害逻辑
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
            
            // 触发受击无敌
            player.AddComponent(new InvincibleComponent { RemainingTime = config.PlayerInvincibleDuration });
            
            Debug.Log($"[Collision] 基于法线反弹！玩家剩余血量: {pHealth.CurrentHealth}");
        }
    }
}