using System.Collections.Generic;
using UnityEngine;

// 打上这个标签，意味着这是一次纯物理的“肉弹战车”式反弹，落地后不需要受击罚站
public class PhysicalBounceTag : Component { }

/// <summary>
/// 物理击退与碰撞挤压系统
/// 优化：修复了 Source/Target 乱序导致的漏判 Bug，确保每次碰撞必定弹开
/// </summary>
public class KnockbackSystem : SystemBase 
{
    public KnockbackSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime) 
    {
        var hitEvents = GetEntitiesWith<CollisionEventComponent>();

        // 1. 处理碰撞瞬间的物理排斥
        foreach (var entity in hitEvents) 
        {
            if (entity.HasComponent<BulletTag>()) continue;

            var evt = entity.GetComponent<CollisionEventComponent>();
            var eA = evt.Source;
            var eB = evt.Target;

            if (eA == null || !eA.IsAlive || eB == null || !eB.IsAlive) continue;

            bool aIsPlayer = eA.HasComponent<PlayerTag>();
            bool bIsPlayer = eB.HasComponent<PlayerTag>();
            bool aIsEnemy = eA.HasComponent<EnemyTag>();
            bool bIsEnemy = eB.HasComponent<EnemyTag>();

            // ==========================================
            // 【怪物撞墙】：无视事件顺序，精准提纯玩家和怪物实体
            // ==========================================
            if ((aIsPlayer && bIsEnemy) || (bIsPlayer && aIsEnemy))
            {
                // 精准提取
                Entity player = aIsPlayer ? eA : eB;
                Entity enemy = aIsEnemy ? eA : eB;

                var pPos = player.GetComponent<PositionComponent>();
                var ePos = enemy.GetComponent<PositionComponent>();
                var eVel = enemy.GetComponent<VelocityComponent>();

                // 计算反弹方向：从玩家中心指向怪物中心（把怪物往外推）
                Vector2 pushDir = new Vector2(ePos.X - pPos.X, ePos.Y - pPos.Y);
                if (pushDir.sqrMagnitude < 0.0001f) pushDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                pushDir.Normalize();

                float pMass = player.HasComponent<MassComponent>() ? player.GetComponent<MassComponent>().Value : 100f;
                float eMass = enemy.HasComponent<MassComponent>() ? enemy.GetComponent<MassComponent>().Value : 50f;
                float pushRatio = pMass / (pMass + eMass); 

                // 1. 物理坐标强行排斥（防止重叠）
                float hardPush = 0.1f * pushRatio;
                ePos.X += pushDir.x * hardPush;
                ePos.Y += pushDir.y * hardPush;

                // 2. 赋予瞬间爆发动量（玩家不动，只改怪物速度）
                float bounceForce = 15.0f * pushRatio; 
                eVel.VX += pushDir.x * bounceForce;
                eVel.VY += pushDir.y * bounceForce;

                // 3. 挂载 0.15 秒物理滞空状态，彻底阻断 AI 寻路
                if (!enemy.HasComponent<KnockbackComponent>())
                {
                    enemy.AddComponent(new KnockbackComponent { Timer = 0.15f });
                    enemy.AddComponent(new PhysicalBounceTag());
                }
            }
            // ==========================================
            // 【怪物互挤】：同样无视顺序处理软碰撞
            // ==========================================
            else if (aIsEnemy && bIsEnemy)
            {
                var aPos = eA.GetComponent<PositionComponent>();
                var bPos = eB.GetComponent<PositionComponent>();
                var aVel = eA.GetComponent<VelocityComponent>();
                var bVel = eB.GetComponent<VelocityComponent>();

                Vector2 pushDir = new Vector2(bPos.X - aPos.X, bPos.Y - aPos.Y);
                if (pushDir.sqrMagnitude < 0.0001f) pushDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                pushDir.Normalize();

                float swarmPush = 0.03f;
                
                // 将 A 往反方向推
                aPos.X -= pushDir.x * swarmPush;
                aPos.Y -= pushDir.y * swarmPush;
                aVel.VX -= pushDir.x * 0.5f;
                aVel.VY -= pushDir.y * 0.5f;

                // 将 B 往正方向推
                bPos.X += pushDir.x * swarmPush;
                bPos.Y += pushDir.y * swarmPush;
                bVel.VX += pushDir.x * 0.5f;
                bVel.VY += pushDir.y * 0.5f;
            }
        }
        
        // 2. 处理反弹/击退的结束逻辑
        var slidingOnes = GetEntitiesWith<KnockbackComponent>();
        for (int i = slidingOnes.Count - 1; i >= 0; i--)
        {
            var e = slidingOnes[i];
            var kb = e.GetComponent<KnockbackComponent>();
            kb.Timer -= deltaTime;
            
            if (kb.Timer <= 0)
            {
                e.RemoveComponent<KnockbackComponent>();

                if (e.HasComponent<PhysicalBounceTag>())
                {
                    // 纯物理反弹结束，直接恢复 AI 移动
                    e.RemoveComponent<PhysicalBounceTag>();
                }
                else
                {
                    // 其他攻击造成的击退，转入罚站硬直
                    var stats = e.GetComponent<HitRecoveryStatsComponent>();
                    float duration = stats != null ? stats.Duration : 0.2f;
                    e.AddComponent(new HitRecoveryComponent { Timer = duration });
                }
            }
        }
    }
}