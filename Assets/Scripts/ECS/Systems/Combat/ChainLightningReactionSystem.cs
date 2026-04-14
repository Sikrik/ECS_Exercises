// 路径: Assets/Scripts/ECS/Systems/Combat/ChainLightningReactionSystem.cs
using System.Collections.Generic;
using UnityEngine;

public class ChainLightningReactionSystem : SystemBase
{
    public ChainLightningReactionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var hitEvents = GetEntitiesWith<CollisionEventComponent>();

        foreach (var entity in hitEvents)
        {
            var evt = entity.GetComponent<CollisionEventComponent>();
            var bullet = evt.Source;
            var startTarget = evt.Target;

            if (bullet == null || !bullet.IsAlive || !bullet.HasComponent<ChainComponent>()) continue;
            if (startTarget == null || !startTarget.IsAlive || !startTarget.HasComponent<EnemyTag>()) continue;

            var chain = bullet.GetComponent<ChainComponent>();
            var dmg = bullet.GetComponent<DamageComponent>();
            var hitPos = bullet.GetComponent<PositionComponent>();

            List<Entity> hitHistory = new List<Entity> { startTarget };
            Entity currentTarget = startTarget;
            Vector3 lastPos = new Vector3(hitPos.X, hitPos.Y, 0);

            for (int i = 0; i < chain.MaxTargets - 1; i++)
            {
                Entity nextTarget = null;
                float minDistSq = chain.Range * chain.Range;
                var curPos = currentTarget.GetComponent<PositionComponent>();

                var nearby = ECSManager.Instance.Grid.GetNearbyEnemies(curPos.X, curPos.Y);
                foreach (var candidate in nearby)
                {
                    if (!candidate.IsAlive || !candidate.HasComponent<EnemyTag>() || 
                        hitHistory.Contains(candidate) || candidate.HasComponent<InvincibleComponent>()) 
                        continue;
                    
                    var cPos = candidate.GetComponent<PositionComponent>();
                    float d2 = (cPos.X - curPos.X) * (cPos.X - curPos.X) + (cPos.Y - curPos.Y) * (cPos.Y - curPos.Y);
                    if (d2 < minDistSq) 
                    { 
                        minDistSq = d2; 
                        nextTarget = candidate; 
                    }
                }

                if (nextTarget != null)
                {
                    hitHistory.Add(nextTarget);
                    
                    // 👇 【高内聚改造】：剥夺直接扣血的权力，统一抛出伤害意图
                    var existingDmg = nextTarget.GetComponent<DamageEventComponent>();
                    if (existingDmg != null)
                    {
                        existingDmg.DamageAmount += dmg.Value;
                    }
                    else
                    {
                        nextTarget.AddComponent(new DamageEventComponent { 
                            DamageAmount = dmg.Value, 
                            Source = bullet, // 传入子弹作为源头，DamageSystem 会顺藤摸瓜处理
                            IsCritical = false 
                        });
                    }

                    var nPos = nextTarget.GetComponent<PositionComponent>();
                    Vector3 nextPos = new Vector3(nPos.X, nPos.Y, 0);
                    
                    Entity vfxEvent = ECSManager.Instance.CreateEntity();
                    vfxEvent.AddComponent(new VFXSpawnEventComponent { 
                        VFXType = "LightningChain", 
                        Position = lastPos,
                        EndPosition = nextPos 
                    });
                    
                    lastPos = nextPos;
                    currentTarget = nextTarget;
                }
                else break; 
            }
            
            bullet.RemoveComponent<ChainComponent>();
        }
    }
}