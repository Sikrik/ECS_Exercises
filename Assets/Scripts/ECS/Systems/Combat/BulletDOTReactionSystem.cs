// 路径: Assets/Scripts/ECS/Systems/Combat/BulletDOTReactionSystem.cs
using System.Collections.Generic;

public class BulletDOTReactionSystem : SystemBase
{
    public BulletDOTReactionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var hitEvents = GetEntitiesWith<CollisionEventComponent>();

        foreach (var entity in hitEvents)
        {
            var evt = entity.GetComponent<CollisionEventComponent>();
            var bullet = evt.Source;
            var target = evt.Target;

            // 检查子弹是否携带 DOT 负载
            if (bullet == null || !bullet.IsAlive || !bullet.HasComponent<BulletDOTPayloadComponent>()) continue;
            // 检查目标是否是活着的敌人且未处于无敌状态
            if (target == null || !target.IsAlive || !target.HasComponent<EnemyTag>() || target.HasComponent<InvincibleComponent>()) continue;

            var payload = bullet.GetComponent<BulletDOTPayloadComponent>();

            // 赋予 DOTEffectComponent（如果没有的话）
            if (!target.HasComponent<DOTEffectComponent>())
            {
                target.AddComponent(new DOTEffectComponent());
            }
            var dotComp = target.GetComponent<DOTEffectComponent>();

            // 独立刷新或叠加不同类型的 DOT
            if (dotComp.ActiveDOTs.ContainsKey(payload.VfxName))
            {
                var existingDot = dotComp.ActiveDOTs[payload.VfxName];
                existingDot.Duration = payload.Duration;
                existingDot.DamagePerSecond = payload.DPS; // 以最新伤害为准
            }
            else
            {
                dotComp.ActiveDOTs[payload.VfxName] = new DOTEffectComponent.DOTState 
                {
                    DamagePerSecond = payload.DPS,
                    Duration = payload.Duration,
                    TickTimer = 0.5f,
                    VfxName = payload.VfxName
                };

                // 呼叫表现层生成特效 (燃烧/中毒)
                Entity vfxEvent = ECSManager.Instance.CreateEntity();
                vfxEvent.AddComponent(new VFXSpawnEventComponent { 
                    VFXType = payload.VfxName, 
                    AttachTarget = target 
                });
            }

            // 【额外联动】：如果是中毒(PoisonVFX)，联动减速系统！
            if (payload.VfxName == "PoisonVFX" && !target.HasComponent<SlowEffectComponent>())
            {
                // 毒属性额外附加 30% 减速
                target.AddComponent(new SlowEffectComponent(0.3f, payload.Duration));
            }
        }
    }
}