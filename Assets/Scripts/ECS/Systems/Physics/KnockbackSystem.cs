using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 物理击退与碰撞挤压系统
/// 优化：玩家免疫物理位移（叹息之墙）；怪物撞击玩家时，会根据自身重量被物理弹开，且不产生急停硬直。
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
            // 忽略子弹穿透，纯处理肉体碰撞
            if (entity.HasComponent<BulletTag>()) continue;

            var evt = entity.GetComponent<CollisionEventComponent>();
            var target = evt.Target;
            var source = evt.Source;

            if (target == null || !target.IsAlive) continue;
            if (source == null || !source.IsAlive) continue;

            // ==========================================
            // 【玩家霸体】：玩家绝不接受任何碰撞造成的位移和推力
            // ==========================================
            if (target.HasComponent<PlayerTag>()) 
            {
                continue;
            }

            var tPos = target.GetComponent<PositionComponent>();
            var sPos = source.GetComponent<PositionComponent>();
            var tVel = target.GetComponent<VelocityComponent>();

            Vector2 pushDir = new Vector2(tPos.X - sPos.X, tPos.Y - sPos.Y);
            if (pushDir.sqrMagnitude < 0.0001f) pushDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            pushDir.Normalize();

            // ==========================================
            // 【怪物撞墙】：怪物撞击到玩家，根据重量被弹开
            // ==========================================
            if (source.HasComponent<PlayerTag>() && target.HasComponent<EnemyTag>())
            {
                // 获取双方质量（假设玩家重100）
                float sMass = source.HasComponent<MassComponent>() ? source.GetComponent<MassComponent>().Value : 100f;
                float tMass = target.HasComponent<MassComponent>() ? target.GetComponent<MassComponent>().Value : 50f;
                
                // 【核心：动量分配比例】
                // 玩家(sMass)越重，怪物(tMass)越轻，怪物吃到的反弹推力比例就越大
                // 例如：20kg的小怪比例是 100/120 = 0.83；150kg的坦克怪比例是 100/250 = 0.4
                float pushRatio = sMass / (sMass + tMass); 

                // 1. 位置排斥（轻微防重叠）
                float hardPush = 0.1f * pushRatio;
                tPos.X += pushDir.x * hardPush;
                tPos.Y += pushDir.y * hardPush;

                // 2. 赋予基于重量的物理反弹速度（弹开效果）
                // 基础弹力 15.0f，乘以比例。轻怪会被瞬间弹飞，重怪只是微微后退
                float bounceForce = 15.0f * pushRatio; 
                
                // 注意这里是 +=，叠加反弹速度
                tVel.VX += pushDir.x * bounceForce;
                tVel.VY += pushDir.y * bounceForce;

                // 【不急停的关键】：故意不给怪物添加 KnockbackComponent！
                // 因为没有状态组件，怪物的 AI 会一直保持运行。
                // 这股物理弹力会在 MovementSystem 的摩擦力下迅速衰减，而怪物的 AI 会让它刚被弹开就立刻继续往上扑，
                // 形成一种非常有弹性和肉感的“皮球撞墙”体验，完全没有急停罚站的感觉。
            }
            // ==========================================
            // 【怪物互挤】：维持低强度的软碰撞（虫群流动感）
            // ==========================================
            else if (source.HasComponent<EnemyTag>() && target.HasComponent<EnemyTag>())
            {
                float swarmPush = 0.03f;
                tPos.X += pushDir.x * swarmPush;
                tPos.Y += pushDir.y * swarmPush;
                
                tVel.VX += pushDir.x * 0.5f;
                tVel.VY += pushDir.y * 0.5f;
            }
        }
        
        // 2. 处理可能由其他系统（如爆炸技能、武器击退）主动附加的受击硬直
        var slidingOnes = GetEntitiesWith<KnockbackComponent>();
        for (int i = slidingOnes.Count - 1; i >= 0; i--)
        {
            var e = slidingOnes[i];
            var kb = e.GetComponent<KnockbackComponent>();
            kb.Timer -= deltaTime;
            
            if (kb.Timer <= 0)
            {
                e.RemoveComponent<KnockbackComponent>();
                // 靠摩擦力平滑减速，不再强制 vel=0 急停
                var stats = e.GetComponent<HitRecoveryStatsComponent>();
                float duration = stats != null ? stats.Duration : 0.2f;
                e.AddComponent(new HitRecoveryComponent { Timer = duration });
            }
        }
    }
}