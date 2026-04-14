// 路径: Assets/Scripts/ECS/Systems/Combat/ExplosionSystem.cs
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
                // 加入无敌判断，无敌状态下不受到爆炸伤害
                if (!t.IsAlive || !t.HasComponent<EnemyTag>() || t.HasComponent<InvincibleComponent>()) continue;
                
                var tPos = t.GetComponent<PositionComponent>();
                float d2 = (tPos.X - pos.X) * (tPos.X - pos.X) + (tPos.Y - pos.Y) * (tPos.Y - pos.Y);

                if (d2 <= rSq)
                {
                    // 👇 【高内聚改造】：剥夺直接扣血的权力，改为抛出标准的伤害意图
                    var existingDmg = t.GetComponent<DamageEventComponent>();
                    if (existingDmg != null)
                    {
                        // 累加伤害，防止同帧内被多个爆炸波及导致伤害覆盖
                        existingDmg.DamageAmount += intent.Damage;
                    }
                    else
                    {
                        t.AddComponent(new DamageEventComponent { 
                            DamageAmount = intent.Damage, 
                            Source = null, // 注意：如果爆炸需要触发玩家吸血，这里可以传入产生爆炸的源头实体
                            IsCritical = false 
                        });
                    }
                }
            }

            exp.AddComponent(new PendingDestroyComponent());
        }
    }
}