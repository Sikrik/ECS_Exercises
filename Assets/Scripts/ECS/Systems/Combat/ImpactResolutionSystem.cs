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
            // 场景 1：玩家 vs 敌人 (双向弹开 + 双向硬直)
            // ========================================================
            if ((isSourcePlayer && isTargetEnemy) || (isSourceEnemy && isTargetPlayer))
            {
                Entity player = isSourcePlayer ? source : target;
                Entity enemy = isSourceEnemy ? source : target;

                var pPos = player.GetComponent<PositionComponent>();
                var ePos = enemy.GetComponent<PositionComponent>();
                Vector2 dirToEnemy = new Vector2(ePos.X - pPos.X, ePos.Y - pPos.Y).normalized;
                
                // --- 1. 玩家受到反弹 ---
                var pVel = player.GetComponent<VelocityComponent>();
                if (pVel != null)
                {
                    pVel.VX = -dirToEnemy.x * 12f; // 反冲力度
                    pVel.VY = -dirToEnemy.y * 12f;
                }
                if (!player.HasComponent<KnockbackComponent>())
                {
                    player.AddComponent(new KnockbackComponent { Timer = 0.15f, HitRecoveryAfterwards = 0.1f });
                }

                // --- 2. 敌人受到反弹 ---
                var eVel = enemy.GetComponent<VelocityComponent>();
                if (eVel != null)
                {
                    float bounceForce = enemy.HasComponent<BounceForceComponent>() ? enemy.GetComponent<BounceForceComponent>().Value : 15f;
                    eVel.VX = dirToEnemy.x * bounceForce;
                    eVel.VY = dirToEnemy.y * bounceForce;
                }
                if (!enemy.HasComponent<KnockbackComponent>())
                {
                    float eRecovery = enemy.HasComponent<HitRecoveryStatsComponent>() ? enemy.GetComponent<HitRecoveryStatsComponent>().Duration : 0.2f;
                    enemy.AddComponent(new KnockbackComponent { Timer = 0.2f, HitRecoveryAfterwards = eRecovery });
                }
                continue; 
            }

            // ========================================================
            // 场景 2：敌人 vs 敌人 (怪物互挤，消除抖动)
            // 规则：不碰 Velocity，直接平滑修正 Position，实现贴边滑动
            // ========================================================
            else if (isSourceEnemy && isTargetEnemy)
            {
                var pos1 = source.GetComponent<PositionComponent>();
                var pos2 = target.GetComponent<PositionComponent>();
                
                Vector2 dir = new Vector2(pos1.X - pos2.X, pos1.Y - pos2.Y);
                if (dir.sqrMagnitude < 0.001f) 
                {
                    dir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                }
                dir.Normalize();

                // 平滑纠正坐标，不再引发惯性系统计算
                float slideSpeed = 2.0f * deltaTime; 
                pos1.X += dir.x * slideSpeed;
                pos1.Y += dir.y * slideSpeed;
                pos2.X -= dir.x * slideSpeed;
                pos2.Y -= dir.y * slideSpeed;
                
                continue;
            }

            // ========================================================
            // 场景 3：子弹 vs 任何活物
            // ========================================================
            if (isSourceBullet || isTargetBullet)
            {
                Entity bullet = isSourceBullet ? source : target;
                Entity victim = isSourceBullet ? target : source;
                
                var bFaction = bullet.GetComponent<FactionComponent>();
                var vFaction = victim.GetComponent<FactionComponent>();

                if (bFaction != null && vFaction != null && bFaction.Value == vFaction.Value)
                {
                    continue; 
                }

                // 无物理操作，全权交由 DamageSystem 处理
                continue; 
            }
        }
        
        ReturnListToPool(hitEvents);
    }
}