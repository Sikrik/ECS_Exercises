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

            // 1. 表现层意图：爆炸特效
            Entity vfxEvent = ECSManager.Instance.CreateEntity();
            vfxEvent.AddComponent(new VFXSpawnEventComponent { 
                VFXType = "Explosion", 
                Position = new UnityEngine.Vector3(pos.X, pos.Y, 0) 
            });

            // 2. 核心搜索与伤害逻辑（高内聚）
            var targets = ECSManager.Instance.Grid.GetNearbyEnemies(pos.X, pos.Y);
            float rSq = intent.Radius * intent.Radius;

            foreach (var t in targets)
            {
                if (!t.IsAlive || !t.HasComponent<EnemyTag>()) continue;
                var tPos = t.GetComponent<PositionComponent>();
                float d2 = (tPos.X - pos.X) * (tPos.X - pos.X) + (tPos.Y - pos.Y) * (tPos.Y - pos.Y);

                if (d2 <= rSq)
                {
                    t.AddComponent(EventPool.GetDamageEvent(intent.Damage, false));
                }
            }

            // 爆炸是瞬时的
            exp.AddComponent(new PendingDestroyComponent());
        }
    }
}