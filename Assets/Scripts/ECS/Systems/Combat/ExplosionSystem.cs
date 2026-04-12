using System.Collections.Generic;

public class ExplosionSystem : SystemBase
{
    public ExplosionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var explosions = GetEntitiesWith<ExplosionIntentComponent, PositionComponent>();

        foreach (var exp in explosions)
        {
            var intent = exp.GetComponent<ExplosionIntentComponent>();
            var pos = exp.GetComponent<PositionComponent>();

            Entity vfxEvent = ECSManager.Instance.CreateEntity();
            vfxEvent.AddComponent(new VFXSpawnEventComponent { 
                VFXType = "Explosion", 
                Position = new UnityEngine.Vector3(pos.X, pos.Y, 0) 
            });

            var targets = ECSManager.Instance.Grid.GetNearbyEnemies(pos.X, pos.Y);
            float rSq = intent.Radius * intent.Radius;

            foreach (var t in targets)
            {
                // 加入无敌判断
                if (!t.IsAlive || !t.HasComponent<EnemyTag>() || t.HasComponent<InvincibleComponent>()) continue;
                
                var tPos = t.GetComponent<PositionComponent>();
                float d2 = (tPos.X - pos.X) * (tPos.X - pos.X) + (tPos.Y - pos.Y) * (tPos.Y - pos.Y);

                if (d2 <= rSq)
                {
                    // 👇 [核心修复]：真实扣除血量
                    var hp = t.GetComponent<HealthComponent>();
                    if (hp != null) hp.CurrentHealth -= intent.Damage;

                    // 👇 [核心修复]：累加防覆盖
                    var existingEvt = t.GetComponent<DamageTakenEventComponent>();
                    if (existingEvt != null)
                    {
                        existingEvt.DamageAmount += intent.Damage;
                    }
                    else
                    {
                        t.AddComponent(EventPool.GetDamageEvent(intent.Damage, false));
                    }
                }
            }

            exp.AddComponent(new PendingDestroyComponent());
        }
    }
}