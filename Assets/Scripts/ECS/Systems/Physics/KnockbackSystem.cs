using System.Collections.Generic;
using UnityEngine;

public class KnockbackSystem : SystemBase 
{
    public KnockbackSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime) 
    {
        var hitEvents = GetEntitiesWith<CollisionEventComponent>();

        foreach (var entity in hitEvents) 
        {
            // 忽略子弹穿透，纯处理肉体碰撞
            if (entity.HasComponent<BulletTag>()) continue;

            var evt = entity.GetComponent<CollisionEventComponent>();
            var target = evt.Target;
            var source = evt.Source; // 撞击者

            // 确保双方都有物理实体和质量
            if (target == null || !target.IsAlive || !target.HasComponent<MassComponent>()) continue;
            if (source == null || !source.IsAlive || !source.HasComponent<MassComponent>()) continue;

            var tPos = target.GetComponent<PositionComponent>();
            var sPos = source.GetComponent<PositionComponent>();
            var tVel = target.GetComponent<VelocityComponent>();

            // 【手感核心1】：计算真实的排斥方向，抛弃偶尔会出 Bug 的底层 Normal
            Vector2 pushDir = new Vector2(tPos.X - sPos.X, tPos.Y - sPos.Y);
            if (pushDir.sqrMagnitude < 0.0001f) pushDir = new Vector2(Random.Range(-1f,1f), Random.Range(-1f,1f));
            pushDir.Normalize();

            // 【手感核心2】：动量守恒比例（对方越重，我被推得越狠）
            float sMass = source.GetComponent<MassComponent>().Value;
            float tMass = target.GetComponent<MassComponent>().Value;
            float pushRatio = sMass / (sMass + tMass); 

            // 判断是否是怪物互相挤压
            bool isEnemySwarm = source.HasComponent<EnemyTag>() && target.HasComponent<EnemyTag>();

            if (isEnemySwarm)
            {
                // 【软碰撞】：怪物互挤，仅产生微小的位置分离，不触发硬直打断
                float swarmPush = 0.05f * pushRatio;
                tPos.X += pushDir.x * swarmPush;
                tPos.Y += pushDir.y * swarmPush;

                // 给一点点速度分离，保持虫群的流动性
                tVel.VX += pushDir.x * 0.5f;
                tVel.VY += pushDir.y * 0.5f;
            }
            else
            {
                // 【硬碰撞】：玩家与怪物的肉搏，打击感强
                // 1. 位置硬修正，防止穿墙
                float hardPush = Mathf.Clamp(0.2f * pushRatio, 0.02f, 0.2f);
                tPos.X += pushDir.x * hardPush;
                tPos.Y += pushDir.y * hardPush;

                // 2. 动量弹开
                float bounceForce = target.HasComponent<BounceForceComponent>() ? target.GetComponent<BounceForceComponent>().Value : 8.0f;
                float finalForce = bounceForce * pushRatio * 2f; // 放大系数，让反弹更明显
                
                tVel.VX = pushDir.x * finalForce;
                tVel.VY = pushDir.y * finalForce;

                // 3. 施加击退状态 (打断移动和AI寻路)
                if (!target.HasComponent<KnockbackComponent>())
                {
                    // 击退时间变短（0.15秒），让动作游戏的节奏更脆，不会“飞半天”
                    target.AddComponent(new KnockbackComponent { Timer = 0.15f });
                }
            }
        }
        
        // --- 下面处理滑动结束转硬直的逻辑保持不变 ---
        var slidingOnes = GetEntitiesWith<KnockbackComponent>();
        for (int i = slidingOnes.Count - 1; i >= 0; i--)
        {
            var e = slidingOnes[i];
            var kb = e.GetComponent<KnockbackComponent>();
            kb.Timer -= deltaTime;
            if (kb.Timer <= 0)
            {
                e.RemoveComponent<KnockbackComponent>();
                // 转入原地硬直
                var stats = e.GetComponent<HitRecoveryStatsComponent>();
                float duration = stats != null ? stats.Duration : 0.2f; // 默认硬直也缩短
                e.AddComponent(new HitRecoveryComponent { Timer = duration });
                var vel = e.GetComponent<VelocityComponent>();
                if (vel != null) { vel.VX = 0; vel.VY = 0; }
            }
        }
    }
}