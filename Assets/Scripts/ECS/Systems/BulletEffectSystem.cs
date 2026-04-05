using System.Collections.Generic;
using UnityEngine;

public class BulletEffectSystem : SystemBase
{
    public BulletEffectSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var hitBullets = GetEntitiesWith<BulletHitEventComponent, BulletComponent, PositionComponent>();
        var ecs = ECSManager.Instance;

        foreach (var bullet in hitBullets)
        {
            var hitEvent = bullet.GetComponent<BulletHitEventComponent>();
            var bulletComp = bullet.GetComponent<BulletComponent>();
            var bPos = bullet.GetComponent<PositionComponent>();

            if (hitEvent.Target != null && hitEvent.Target.IsAlive)
            {
                // 扣血逻辑
                var health = hitEvent.Target.GetComponent<HealthComponent>();
                if (health != null) health.CurrentHealth -= bulletComp.Damage;

                // 特效逻辑
                if (bulletComp.Type == BulletType.ChainLightning)
                {
                    TriggerChainLightning(hitEvent.Target, bPos);
                }
            }
            ecs.DestroyEntity(bullet);
        }
    }

    private void TriggerChainLightning(Entity target, PositionComponent startPos)
    {
        var ecs = ECSManager.Instance;
        var config = ecs.Config;
        var enemies = GetEntitiesWith<EnemyComponent, PositionComponent, HealthComponent>();
        
        Entity current = target;
        Vector3 lastPos = new Vector3(startPos.X, startPos.Y, 0);

        for (int i = 0; i < config.ChainLightningMaxTargets - 1; i++)
        {
            Entity next = null;
            float minDistSq = config.ChainLightningChainRange * config.ChainLightningChainRange;
            var curPosComp = current.GetComponent<PositionComponent>();

            foreach (var e in enemies)
            {
                if (!e.IsAlive || e == current) continue;
                var ePos = e.GetComponent<PositionComponent>();
                float d2 = (ePos.X - curPosComp.X) * (ePos.X - curPosComp.X) + (ePos.Y - curPosComp.Y) * (ePos.Y - curPosComp.Y);
                if (d2 < minDistSq) { minDistSq = d2; next = e; }
            }

            if (next != null)
            {
                next.GetComponent<HealthComponent>().CurrentHealth -= config.ChainLightningDamage;
                var nextPos = next.GetComponent<PositionComponent>();
                
                // 创建视觉实体
                Entity vfx = ecs.CreateEntity();
                vfx.AddComponent(new LightningVFXComponent(lastPos, new Vector3(nextPos.X, nextPos.Y, 0)));
                vfx.AddComponent(new ViewComponent(ecs.ChainLightningBulletPool.Get()));
                
                current = next;
                lastPos = new Vector3(nextPos.X, nextPos.Y, 0);
            }
            else break;
        }
    }
}