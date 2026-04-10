using System.Collections.Generic;
using UnityEngine;

// 纯物理反弹标签：用于区分是“撞墙弹开”还是“被武器打退”
public class PhysicalBounceTag : Component { }

/// <summary>
/// 物理击退与碰撞挤压系统
/// 包含：玩家霸体、基于质量的动量分配、无视事件顺序的精准匹配、以及丝滑的Lerp减速刹车
/// </summary>
public class KnockbackSystem : SystemBase 
{
    public KnockbackSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime) 
    {
        var hitEvents = GetEntitiesWith<CollisionEventComponent>();

        // ==========================================
        // 1. 处理碰撞瞬间的物理排斥与初速度赋予
        // ==========================================
        foreach (var entity in hitEvents) 
        {
            // 忽略子弹的穿透事件，纯处理肉体碰撞
            if (entity.HasComponent<BulletTag>()) continue;

            var evt = entity.GetComponent<CollisionEventComponent>();
            var eA = evt.Source;
            var eB = evt.Target;

            if (eA == null || !eA.IsAlive || eB == null || !eB.IsAlive) continue;

            bool aIsPlayer = eA.HasComponent<PlayerTag>();
            bool bIsPlayer = eB.HasComponent<PlayerTag>();
            bool aIsEnemy = eA.HasComponent<EnemyTag>();
            bool bIsEnemy = eB.HasComponent<EnemyTag>();

            // 【情况 A：怪物撞击玩家】 -> 瞬间物理弹开
            if ((aIsPlayer && bIsEnemy) || (bIsPlayer && aIsEnemy))
            {
                Entity player = aIsPlayer ? eA : eB;
                Entity enemy = aIsEnemy ? eA : eB;

                var pPos = player.GetComponent<PositionComponent>();
                var ePos = enemy.GetComponent<PositionComponent>();
                var eVel = enemy.GetComponent<VelocityComponent>();

                // 计算反弹方向（从玩家中心推向怪物）
                Vector2 pushDir = new Vector2(ePos.X - pPos.X, ePos.Y - pPos.Y);
                if (pushDir.sqrMagnitude < 0.0001f) pushDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                pushDir.Normalize();

                // 获取质量计算反弹比例
                float pMass = player.HasComponent<MassComponent>() ? player.GetComponent<MassComponent>().Value : 100f;
                float eMass = enemy.HasComponent<MassComponent>() ? enemy.GetComponent<MassComponent>().Value : 50f;
                float pushRatio = pMass / (pMass + eMass); 

                // 1. 空间坐标防重叠硬推
                float hardPush = 0.1f * pushRatio;
                ePos.X += pushDir.x * hardPush;
                ePos.Y += pushDir.y * hardPush;

                // 2. 赋予极高的初始物理反弹速度
                float bounceForce = 15.0f * pushRatio; 
                eVel.VX += pushDir.x * bounceForce;
                eVel.VY += pushDir.y * bounceForce;

                // 3. 挂载 0.15 秒物理滞空状态，剥夺 AI 控制权
                if (!enemy.HasComponent<KnockbackComponent>())
                {
                    enemy.AddComponent(new KnockbackComponent { Timer = 0.15f });
                    enemy.AddComponent(new PhysicalBounceTag());
                }
            }
            // 【情况 B：怪物互挤】 -> 虫群软碰撞流动
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
                
                aPos.X -= pushDir.x * swarmPush;
                aPos.Y -= pushDir.y * swarmPush;
                aVel.VX -= pushDir.x * 0.5f;
                aVel.VY -= pushDir.y * 0.5f;

                bPos.X += pushDir.x * swarmPush;
                bPos.Y += pushDir.y * swarmPush;
                bVel.VX += pushDir.x * 0.5f;
                bVel.VY += pushDir.y * 0.5f;
            }
        }
        
        // ==========================================
        // 2. 处理击退滑行的平滑减速(Lerp)与刹车罚站
        // ==========================================
        var slidingOnes = GetEntitiesWith<KnockbackComponent>();
        for (int i = slidingOnes.Count - 1; i >= 0; i--)
        {
            var e = slidingOnes[i];
            var kb = e.GetComponent<KnockbackComponent>();
            kb.Timer -= deltaTime;
            
            var vel = e.GetComponent<VelocityComponent>();
            if (vel != null)
            {
                // 【核心手感：丝滑减速】使用 Lerp 代替暴力的乘法摩擦力，数值 15f 越大刹车越快
                vel.VX = Mathf.Lerp(vel.VX, 0, deltaTime * 15f);
                vel.VY = Mathf.Lerp(vel.VY, 0, deltaTime * 15f);
            }

            // 滑行时间结束，稳稳落地
            if (kb.Timer <= 0)
            {
                e.RemoveComponent<KnockbackComponent>();

                // 【核心手感：杜绝溜冰】彻底清空残留的极小速度，让怪物死死钉在原地
                if (vel != null)
                {
                    vel.VX = 0;
                    vel.VY = 0;
                }

                // 结算落地后的硬直（眩晕/罚站）
                if (e.HasComponent<PhysicalBounceTag>())
                {
                    // 肉体撞墙导致的弹开，落地后眩晕 0.3 秒
                    e.RemoveComponent<PhysicalBounceTag>();
                    e.AddComponent(new HitRecoveryComponent { Timer = 0.3f });
                }
                else
                {
                    // 其他攻击造成的击退，读取配置时间进入硬直
                    var stats = e.GetComponent<HitRecoveryStatsComponent>();
                    float duration = stats != null ? stats.Duration : 0.2f;
                    e.AddComponent(new HitRecoveryComponent { Timer = duration });
                }
            }
        }
        ReturnListToPool(slidingOnes);
    }
}