using System.Collections.Generic;
using UnityEngine;

// 【新增】：打上这个标签，意味着这是一次纯物理的“肉弹战车”式反弹，落地后不需要受击罚站
public class PhysicalBounceTag : Component { }

/// <summary>
/// 物理击退与碰撞挤压系统
/// 优化：修复了AI与物理拉扯导致的“停顿感”，实现丝滑的动量反弹
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
            var target = evt.Target;
            var source = evt.Source;

            if (target == null || !target.IsAlive) continue;
            if (source == null || !source.IsAlive) continue;

            // 玩家叹息之墙，绝不动摇
            if (target.HasComponent<PlayerTag>()) continue;

            var tPos = target.GetComponent<PositionComponent>();
            var sPos = source.GetComponent<PositionComponent>();
            var tVel = target.GetComponent<VelocityComponent>();

            Vector2 pushDir = new Vector2(tPos.X - sPos.X, tPos.Y - sPos.Y);
            if (pushDir.sqrMagnitude < 0.0001f) pushDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            pushDir.Normalize();

            // ==========================================
            // 【怪物撞墙】：完美解决停顿感
            // ==========================================
            if (source.HasComponent<PlayerTag>() && target.HasComponent<EnemyTag>())
            {
                float sMass = source.HasComponent<MassComponent>() ? source.GetComponent<MassComponent>().Value : 100f;
                float tMass = target.HasComponent<MassComponent>() ? target.GetComponent<MassComponent>().Value : 50f;
                float pushRatio = sMass / (sMass + tMass); 

                float hardPush = 0.1f * pushRatio;
                tPos.X += pushDir.x * hardPush;
                tPos.Y += pushDir.y * hardPush;

                float bounceForce = 15.0f * pushRatio; 
                tVel.VX += pushDir.x * bounceForce;
                tVel.VY += pushDir.y * bounceForce;

                // 【核心修复】：必须给怪物上一个极短的控制状态（0.15秒），用来彻底挂起AI！
                // 这样物理初速度才能完美生效，怪物会像皮球一样瞬间弹出去
                if (!target.HasComponent<KnockbackComponent>())
                {
                    target.AddComponent(new KnockbackComponent { Timer = 0.15f });
                    // 打上免罪金牌：告诉结算系统，0.15秒后不要给它上硬直
                    target.AddComponent(new PhysicalBounceTag());
                }
            }
            // 怪物互挤：保持原本的一帧软排斥，AI覆盖刚好能形成虫群流动感
            else if (source.HasComponent<EnemyTag>() && target.HasComponent<EnemyTag>())
            {
                float swarmPush = 0.03f;
                tPos.X += pushDir.x * swarmPush;
                tPos.Y += pushDir.y * swarmPush;
                
                tVel.VX += pushDir.x * 0.5f;
                tVel.VY += pushDir.y * 0.5f;
            }
        }
        
        // 2. 处理击退/反弹的结束逻辑
        var slidingOnes = GetEntitiesWith<KnockbackComponent>();
        for (int i = slidingOnes.Count - 1; i >= 0; i--)
        {
            var e = slidingOnes[i];
            var kb = e.GetComponent<KnockbackComponent>();
            kb.Timer -= deltaTime;
            
            if (kb.Timer <= 0)
            {
                e.RemoveComponent<KnockbackComponent>();

                // 【核心修复】：根据标签分流
                if (e.HasComponent<PhysicalBounceTag>())
                {
                    // 纯粹的撞墙反弹，落地瞬间撕毁标签，无缝切回AI，绝不罚站！
                    e.RemoveComponent<PhysicalBounceTag>();
                }
                else
                {
                    // 被子弹或武器打出的击退，按老规矩进入受击罚站硬直
                    var stats = e.GetComponent<HitRecoveryStatsComponent>();
                    float duration = stats != null ? stats.Duration : 0.2f;
                    e.AddComponent(new HitRecoveryComponent { Timer = duration });
                }
            }
        }
    }
}