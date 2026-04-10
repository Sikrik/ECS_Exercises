using System.Collections.Generic;
using UnityEngine;

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
            // 场景 1：玩家 vs 敌人 (谁是Source谁是Target不一定，我们统一提取)
            // 规则：玩家不动（但会受伤，由DamageSystem处理），敌人被弹开 -> 减速 -> 硬直
            // ========================================================
            if ((isSourcePlayer && isTargetEnemy) || (isSourceEnemy && isTargetPlayer))
            {
                Entity player = isSourcePlayer ? source : target;
                Entity enemy = isSourceEnemy ? source : target;

                // 防穿透：把敌人推离玩家一点点，不改变玩家坐标
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

                // 给敌人挂载击退状态，并在 0.2 秒滑行结束后，衔接 0.5 秒的硬直！
                if (!enemy.HasComponent<KnockbackComponent>())
                {
                    enemy.AddComponent(new KnockbackComponent { 
                        Timer = 0.2f, 
                        HitRecoveryAfterwards = enemy.GetComponent<HitRecoveryStatsComponent>().Duration 
                    });
                }
                continue; // 玩家与敌人处理完毕
            }

            // ========================================================
            // 场景 2 & 3：子弹 vs 敌人 / 玩家
            // 规则：无硬直无弹开，仅扣血。利用 Faction 判断是否造成伤害
            // ========================================================
            if (isSourceBullet || isTargetBullet)
            {
                Entity bullet = isSourceBullet ? source : target;
                Entity victim = isSourceBullet ? target : source;
                
                var bFaction = bullet.GetComponent<FactionComponent>();
                var vFaction = victim.GetComponent<FactionComponent>();

                // 如果阵营相同（比如玩家子弹打到玩家），忽略碰撞
                if (bFaction != null && vFaction != null && bFaction.Value == vFaction.Value)
                {
                    continue; 
                }

                // 如果阵营不同，由 DamageSystem 去处理扣血，这里我们不做任何物理击退
                // 如果未来需要某些特殊子弹（如霰弹枪）有击退，可以在这里读取子弹的特殊组件
                continue; 
            }
        }
    }
}