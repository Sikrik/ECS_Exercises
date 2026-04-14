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

            // 如果敌人身上已经有 DOT，刷新时间或叠加伤害（这里采取覆盖刷新机制）
            if (target.HasComponent<DOTEffectComponent>())
            {
                var existingDot = target.GetComponent<DOTEffectComponent>();
                // 如果是同类型 DOT，刷新持续时间
                if (existingDot.VfxName == payload.VfxName) 
                {
                    existingDot.Duration = payload.Duration;
                    existingDot.DamagePerSecond = payload.DPS; // 以最新伤害为准
                }
            }
            else
            {
                // 给敌人挂上持续掉血状态
                target.AddComponent(new DOTEffectComponent(payload.DPS, payload.Duration, payload.VfxName));

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