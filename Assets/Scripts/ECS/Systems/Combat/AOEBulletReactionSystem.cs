using System.Collections.Generic;
using UnityEngine;

public class AOEBulletReactionSystem : SystemBase
{
    public AOEBulletReactionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var hitEvents = GetEntitiesWith<CollisionEventComponent>();

        foreach (var entity in hitEvents)
        {
            var evt = entity.GetComponent<CollisionEventComponent>();
            var bullet = evt.Source;
            var target = evt.Target;
            
            if (bullet == null || !bullet.IsAlive || !bullet.HasComponent<AOEComponent>()) continue;
            if (target == null || !target.IsAlive || !target.HasComponent<EnemyTag>()) continue;

            // 【职责精简】：只负责触发爆炸实体，不再自己搜索敌人
            var aoe = bullet.GetComponent<AOEComponent>();
            var dmg = bullet.GetComponent<DamageComponent>();
            var pos = bullet.GetComponent<PositionComponent>();

            Entity explosion = ECSManager.Instance.CreateEntity();
            explosion.AddComponent(new PositionComponent(pos.X, pos.Y, 0));
            explosion.AddComponent(new ExplosionIntentComponent(aoe.Radius, dmg.Value));

            bullet.RemoveComponent<AOEComponent>();
        }
    }
}