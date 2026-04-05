using System.Collections.Generic;
using UnityEngine;

public class BulletEffectSystem : SystemBase
{
    public BulletEffectSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 处理所有“已命中”的子弹
        var hitBullets = GetEntitiesWith<BulletHitEventComponent, BulletComponent, PositionComponent>();
        var ecs = ECSManager.Instance;

        foreach (var bullet in hitBullets)
        {
            var hitEvent = bullet.GetComponent<BulletHitEventComponent>();
            var bulletComp = bullet.GetComponent<BulletComponent>();
            var bPos = bullet.GetComponent<PositionComponent>();

            if (hitEvent.Target != null && hitEvent.Target.IsAlive)
            {
                // 1. 数值处理：扣血
                var health = hitEvent.Target.GetComponent<HealthComponent>();
                if (health != null) health.CurrentHealth -= bulletComp.Damage;

                // 2. 逻辑处理：如果是闪电链，在此触发寻找下一个目标的逻辑并生成 VFX 实体
                if (bulletComp.Type == BulletType.ChainLightning)
                {
                    TriggerChainLightning(hitEvent.Target, bPos);
                }
            }

            // 3. 命中后销毁子弹
            ecs.DestroyEntity(bullet);
        }
    }

    private void TriggerChainLightning(Entity target, PositionComponent startPos)
    {
        // 逻辑：寻找目标 -> 创建一个带有 LightningVFXComponent 的实体
        // ecs.CreateEntity().AddComponent(new LightningVFXComponent(...));
    }
}