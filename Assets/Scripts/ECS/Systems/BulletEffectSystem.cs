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
        var ecs = ECSManager.Instance;
        var config = ecs.Config;
        var allEnemies = GetEntitiesWith<EnemyComponent, PositionComponent, HealthComponent>();
    
        Entity current = target;
        Vector3 currentVfxStart = new Vector3(startPos.X, startPos.Y, 0);

        // 闪电跳跃逻辑
        for (int i = 0; i < config.ChainLightningMaxTargets - 1; i++)
        {
            Entity next = null;
            float minDistSq = config.ChainLightningChainRange * config.ChainLightningChainRange;
            var curPosComp = current.GetComponent<PositionComponent>();

            foreach (var enemy in allEnemies)
            {
                if (!enemy.IsAlive || enemy == current) continue;
                var ePos = enemy.GetComponent<PositionComponent>();
                float d2 = Vector2.SqrMagnitude(new Vector2(ePos.X - curPosComp.X, ePos.Y - curPosComp.Y));

                if (d2 < minDistSq)
                {
                    minDistSq = d2;
                    next = enemy;
                }
            }

            if (next != null)
            {
                // 1. 逻辑效果：扣血
                next.GetComponent<HealthComponent>().CurrentHealth -= config.ChainLightningDamage;
            
                // 2. 表现效果：创建纯视觉实体
                var nextPos = next.GetComponent<PositionComponent>();
                Entity vfxEntity = ecs.CreateEntity();
                vfxEntity.AddComponent(new LightningVFXComponent(
                    currentVfxStart, 
                    new Vector3(nextPos.X, nextPos.Y, 0)));
            
                // 从池中获取特效对象
                GameObject go = ecs.ChainLightningBulletPool.Get(); // 假设你用该池存放 LineRenderer 物体
                vfxEntity.AddComponent(new ViewComponent(go));

                current = next;
                currentVfxStart = new Vector3(nextPos.X, nextPos.Y, 0);
            }
            else break;
        }
    }
}