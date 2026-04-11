using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 闪电链子弹反应系统（原子化）
/// </summary>
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

            // 执行闪电链算法
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
                    if (!candidate.IsAlive || !candidate.HasComponent<EnemyTag>() || hitHistory.Contains(candidate)) continue;
                    
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
                    
                    // 下发规范的伤害事件
                    nextTarget.AddComponent(EventPool.GetDamageEvent(dmg.Value, causeHitRecovery: false));

                    var nPos = nextTarget.GetComponent<PositionComponent>();
                    Vector3 nextPos = new Vector3(nPos.X, nPos.Y, 0);
                    
                    // 抛出闪电线段特效意图
                    Entity vfxEvent = ECSManager.Instance.CreateEntity();
                    vfxEvent.AddComponent(new VFXSpawnEventComponent { 
                        VFXType = "LightningChain", 
                        Position = lastPos,
                        EndPosition = nextPos 
                    });
                    
                    lastPos = nextPos;
                    currentTarget = nextTarget;
                }
                else break; // 范围内没敌人了，中断弹射
            }
            
            // 触发完弹射后移除组件，防止穿透时重复触发闪电链
            bullet.RemoveComponent<ChainComponent>();
        }
        ReturnListToPool(hitEvents);
    }
}