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
            // 1. 物理防重叠 (挤压分离)
            // 必须放在阵营判断之前！这样同阵营的怪物互相挤压也会生效，防止穿模
            // ==========================================
            if (!source.HasComponent<BulletTag>())
            {
                var sourcePos = source.GetComponent<PositionComponent>();
                var targetPos = target.GetComponent<PositionComponent>();

                if (sourcePos != null && targetPos != null)
                {
                    float pushDist = BattleManager.Instance.Config.CollisionPushDistance; 
                    sourcePos.X -= evt.Normal.x * pushDist;
                    sourcePos.Y -= evt.Normal.y * pushDist;
                }
            }

            // ==========================================
            // 2. 阵营过滤 (决定是否造成伤害和击退)
            // ==========================================
            if (source.HasComponent<FactionComponent>() && target.HasComponent<FactionComponent>())
            {
                if (source.GetComponent<FactionComponent>().Value == target.GetComponent<FactionComponent>().Value)
                {
                    continue; // 同阵营不互相造成伤害和击退
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
                    var dmgEvt = target.GetComponent<DamageEventComponent>();
                    dmgEvt.DamageAmount += actualDmg;
                }
            }
        }
    }
}