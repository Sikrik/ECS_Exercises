using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 碰撞解析系统 (Data-Driven 高内聚重构版)
/// 职责：仅负责计算有质量的物理实体之间的挤压和弹开反馈。
/// 优势：彻底消灭 PlayerTag/EnemyTag 判断，纯靠组件数据运算，完美支持未来添加的友军 NPC 或多阵营大乱斗。
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

            // 防御性校验
            if (source == null || !source.IsAlive || target == null || !target.IsAlive) continue;

            // ========================================================
            // 1. 物理实体过滤：剥离不具备“质量”的物体（例如子弹）
            // ========================================================
            bool sourceIsPhysical = source.HasComponent<MassComponent>();
            bool targetIsPhysical = target.HasComponent<MassComponent>();

            // 只要有一方不是物理实体（如子弹打肉体），直接跳过碰撞排斥
            // 这些非物理碰撞全权交给 DamageSystem 处理伤害即可
            if (!sourceIsPhysical || !targetIsPhysical) 
            {
                continue;
            }

            var fSource = source.GetComponent<FactionComponent>();
            var fTarget = target.GetComponent<FactionComponent>();
            
            var posS = source.GetComponent<PositionComponent>();
            var posT = target.GetComponent<PositionComponent>();

            // 计算从 Source 指向 Target 的向量方向
            Vector2 dirToTarget = new Vector2(posT.X - posS.X, posT.Y - posS.Y);
            if (dirToTarget.sqrMagnitude < 0.001f) 
            {
                dirToTarget = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            }
            dirToTarget.Normalize();

            // ========================================================
            // 2. 场景 A：同阵营碰撞（软排斥平滑滑动）
            // 效果：怪物和怪物之间、玩家和友军之间互相挤开，不产生硬直
            // ========================================================
            if (fSource != null && fTarget != null && fSource.Value == fTarget.Value)
            {
                float slideSpeed = 2.0f * deltaTime; 
                posS.X -= dirToTarget.x * slideSpeed;
                posS.Y -= dirToTarget.y * slideSpeed;
                posT.X += dirToTarget.x * slideSpeed;
                posT.Y += dirToTarget.y * slideSpeed;
                continue;
            }

            // ========================================================
            // 3. 场景 B：敌对阵营碰撞（硬核物理弹开与击退硬直）
            // ========================================================
            var fbSource = source.GetComponent<ImpactFeedbackComponent>();
            var fbTarget = target.GetComponent<ImpactFeedbackComponent>();

            // 规则 1：如果发起方（Source）具有冲撞排斥能力，则把目标（Target）撞飞
            if (fbSource != null && fbSource.CauseBounce)
            {
                ApplyBounce(target, dirToTarget);
            }

            // 规则 2：如果被撞方（Target）也具有冲撞排斥能力，则把发起方（Source）也反向弹开
            // 这就是为什么原来玩家撞怪物，两个人都会各自弹开的原因
            if (fbTarget != null && fbTarget.CauseBounce)
            {
                ApplyBounce(source, -dirToTarget);
            }
        }
        
        ReturnListToPool(hitEvents);
    }

    /// <summary>
    /// 对受害者施加物理弹开和击退状态
    /// </summary>
    /// <param name="victim">被弹开的受害者</param>
    /// <param name="pushDirection">被弹开的方向</param>
    private void ApplyBounce(Entity victim, Vector2 pushDirection)
    {
        var vVel = victim.GetComponent<VelocityComponent>();
        if (vVel != null)
        {
            // 【核心还原】：读取受击方的弹性配置，代表其物理抵抗力
            // 坦克怪 BounceForce=2（弹开极慢），敏捷怪=8（弹开很快），玩家默认无此组件取 fallback=12f（受击反冲明显）
            float force = victim.HasComponent<BounceForceComponent>() 
                ? victim.GetComponent<BounceForceComponent>().Value 
                : 12f; 
            
            vVel.VX = pushDirection.x * force;
            vVel.VY = pushDirection.y * force;
        }

        // 赋予击退与滑行状态（完美对接 KnockbackSystem）
        if (!victim.HasComponent<KnockbackComponent>())
        {
            // 读取配置表里的受击硬直持续时间 (HitRecoveryStatsComponent)，玩家如果没有则默认给个极短的 0.1f 衔接
            float recovery = victim.HasComponent<HitRecoveryStatsComponent>() 
                ? victim.GetComponent<HitRecoveryStatsComponent>().Duration 
                : 0.1f;
            
            // Timer = 0.15f 是滑行减速的时间
            victim.AddComponent(new KnockbackComponent { Timer = 0.15f, HitRecoveryAfterwards = recovery });
        }
    }
}