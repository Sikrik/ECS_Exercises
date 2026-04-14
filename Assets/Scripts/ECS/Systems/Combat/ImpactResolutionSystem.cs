// 路径: Assets/Scripts/ECS/Systems/Combat/ImpactResolutionSystem.cs
using System.Collections.Generic;

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

            if (source == null || !source.IsAlive) continue;
            if (target == null || !target.IsAlive) continue;

            // ==========================================
            // 1. 【必须放在第一位】物理防重叠 (挤压分离)
            // 确保在执行任何 continue 之前，实体就已经被推开了
            // ==========================================
            if (!source.HasComponent<BulletTag>())
            {
                var sourcePos = source.GetComponent<PositionComponent>();
                var targetPos = target.GetComponent<PositionComponent>();

                if (sourcePos != null && targetPos != null)
                {
                    float pushDist = BattleManager.Instance.Config.CollisionPushDistance; 
                    // 这里利用法线将发起方推开，从而解决重叠
                    sourcePos.X -= evt.Normal.x * pushDist;
                    sourcePos.Y -= evt.Normal.y * pushDist;
                }
            }

            // ==========================================
            // 2. 阵营过滤 (拦截同阵营的伤害和击退)
            // 放在挤压逻辑之后。如果是同阵营（如怪物和怪物），走到这里就结束了
            // ==========================================
            if (source.HasComponent<FactionComponent>() && target.HasComponent<FactionComponent>())
            {
                if (source.GetComponent<FactionComponent>().Value == target.GetComponent<FactionComponent>().Value)
                {
                    continue; // 拦截同阵营伤害，直接处理下一对碰撞
                }
            }

            // ==========================================
            // 3. 弹性击退反馈 (Knockback)
            // ==========================================
            if (source.HasComponent<ImpactFeedbackComponent>())
            {
                var feedback = source.GetComponent<ImpactFeedbackComponent>();
                
                if (feedback.CauseBounce && source.HasComponent<BounceForceComponent>())
                {
                    float bounceForce = source.GetComponent<BounceForceComponent>().Value;
                    
                    if (!target.HasComponent<KnockbackComponent>() && !target.HasComponent<InvincibleComponent>())
                    {
                        target.AddComponent(new KnockbackComponent {
                            DirX = evt.Normal.x,
                            DirY = evt.Normal.y,
                            Speed = bounceForce,
                            Timer = 0.15f, 
                            HitRecoveryAfterwards = feedback.HitRecoveryDurationOverride
                        });
                    }
                }
            }

            // ==========================================
            // 4. 产生伤害事件
            // ==========================================
            if (source.HasComponent<DamageComponent>() && target.HasComponent<HealthComponent>())
            {
                if (target.HasComponent<InvincibleComponent>()) continue;

                float actualDmg = source.GetComponent<DamageComponent>().Value;

                if (!target.HasComponent<DamageEventComponent>())
                {
                    target.AddComponent(new DamageEventComponent { 
                        DamageAmount = actualDmg, 
                        Source = source, 
                        IsCritical = false 
                    });
                }
                else
                {
                    // 如果同一帧受多次伤害，进行累加
                    var dmgEvt = target.GetComponent<DamageEventComponent>();
                    dmgEvt.DamageAmount += actualDmg;
                }
            }
        }
    }
}