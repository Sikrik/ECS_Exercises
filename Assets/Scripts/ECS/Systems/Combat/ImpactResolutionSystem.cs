using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 碰撞解析系统：负责将纯粹的碰撞事件转化为物理效果（击退、互推排斥）
/// 不负责扣血（由DamageSystem负责）。
/// </summary>
public class ImpactResolutionSystem : SystemBase
{
    public ImpactResolutionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var hitEvents = GetEntitiesWith<CollisionEventComponent>();

        foreach (var entity in hitEvents)
        {
            var evt = entity.GetComponent<CollisionEventComponent>();
            var source = evt.Source; 
            var target = evt.Target; 

            if (source == null || !source.IsAlive || target == null || !target.IsAlive) continue;

            bool isSourcePlayer = source.HasComponent<PlayerTag>();
            bool isTargetPlayer = target.HasComponent<PlayerTag>();
            bool isSourceEnemy = source.HasComponent<EnemyTag>();
            bool isTargetEnemy = target.HasComponent<EnemyTag>();
            bool isSourceBullet = source.HasComponent<BulletTag>();
            bool isTargetBullet = target.HasComponent<BulletTag>();

            // ========================================================
            // 场景 1：玩家 vs 敌人 
            // 规则：玩家不动，敌人被猛烈弹开 -> 减速滑行 -> 进入硬直
            // ========================================================
            if ((isSourcePlayer && isTargetEnemy) || (isSourceEnemy && isTargetPlayer))
            {
                Entity player = isSourcePlayer ? source : target;
                Entity enemy = isSourceEnemy ? source : target;

                // 防穿透：计算从玩家指向敌人的推离方向
                var pPos = player.GetComponent<PositionComponent>();
                var ePos = enemy.GetComponent<PositionComponent>();
                Vector2 pushDir = new Vector2(ePos.X - pPos.X, ePos.Y - pPos.Y).normalized;
                
                // 给敌人施加物理速度以模拟弹开
                var eVel = enemy.GetComponent<VelocityComponent>();
                if (eVel != null)
                {
                    float bounceForce = enemy.HasComponent<BounceForceComponent>() ? enemy.GetComponent<BounceForceComponent>().Value : 15f;
                    eVel.VX = pushDir.x * bounceForce;
                    eVel.VY = pushDir.y * bounceForce;
                }

                // 给敌人挂载击退状态，并在滑行结束后衔接硬直
                if (!enemy.HasComponent<KnockbackComponent>())
                {
                    enemy.AddComponent(new KnockbackComponent { 
                        Timer = 0.2f, 
                        HitRecoveryAfterwards = enemy.GetComponent<HitRecoveryStatsComponent>().Duration 
                    });
                }
                continue; 
            }

            // ========================================================
            // 场景 2：敌人 vs 敌人 (怪物互挤，防止重叠)
            // 规则：产生微小的互相排斥力，不产生硬直打断
            // ========================================================
            else if (isSourceEnemy && isTargetEnemy)
            {
                var pos1 = source.GetComponent<PositionComponent>();
                var pos2 = target.GetComponent<PositionComponent>();
                
                // 计算两只怪物互相排斥的方向 (从 target 指向 source)
                Vector2 dir = new Vector2(pos1.X - pos2.X, pos1.Y - pos2.Y);
                if (dir.sqrMagnitude < 0.001f) 
                {
                    // 完全重叠时给个随机方向避免死锁
                    dir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                }
                dir.Normalize();

                var vel1 = source.GetComponent<VelocityComponent>();
                var vel2 = target.GetComponent<VelocityComponent>();
                
                // 施加一个微小的持续互斥力 (你可以调整 3.0f 这个数值来改变互推强度)
                float repelForce = 3.0f; 
                if (vel1 != null) { vel1.VX += dir.x * repelForce; vel1.VY += dir.y * repelForce; }
                if (vel2 != null) { vel2.VX -= dir.x * repelForce; vel2.VY -= dir.y * repelForce; }
                
                continue;
            }

            // ========================================================
            // 场景 3：子弹 vs 任何活物
            // 规则：无物理硬直、无弹开、仅扣血（扣血逻辑在 DamageSystem 里处理）
            // ========================================================
            if (isSourceBullet || isTargetBullet)
            {
                Entity bullet = isSourceBullet ? source : target;
                Entity victim = isSourceBullet ? target : source;
                
                var bFaction = bullet.GetComponent<FactionComponent>();
                var vFaction = victim.GetComponent<FactionComponent>();

                // 如果阵营相同（如玩家子弹打到玩家），直接忽略
                if (bFaction != null && vFaction != null && bFaction.Value == vFaction.Value)
                {
                    continue; 
                }

                // 不做任何物理操作，交由后续的 DamageSystem 处理
                continue; 
            }
        }
        
        ReturnListToPool(hitEvents);
    }
}