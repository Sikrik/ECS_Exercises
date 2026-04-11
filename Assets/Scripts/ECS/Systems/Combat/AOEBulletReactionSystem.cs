using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 范围爆炸弹反应系统（原子化）
/// </summary>
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
            
            if (bullet == null || !bullet.IsAlive || !bullet.HasComponent<AOEComponent>()) continue;
            
            // 为了防止同一发 AOE 穿透时连续触发爆炸，要求子弹必须是第一次命中或正在销毁
            if (bullet.HasComponent<PendingDestroyComponent>() || !bullet.HasComponent<PierceComponent>())
            {
                var aoe = bullet.GetComponent<AOEComponent>();
                var dmg = bullet.GetComponent<DamageComponent>();
                var pos = bullet.GetComponent<PositionComponent>();

                // 1. 触发爆炸特效意图
                Entity vfxEvent = ECSManager.Instance.CreateEntity();
                vfxEvent.AddComponent(new VFXSpawnEventComponent { 
                    VFXType = "Explosion", 
                    Position = new Vector3(pos.X, pos.Y, 0) 
                });

                // 2. 利用 GridSystem 查找范围内敌人
                var enemies = ECSManager.Instance.Grid.GetNearbyEnemies(pos.X, pos.Y);
                float rSq = aoe.Radius * aoe.Radius;

                foreach (var target in enemies)
                {
                    if (!target.IsAlive || !target.HasComponent<EnemyTag>()) continue;

                    var tPos = target.GetComponent<PositionComponent>();
                    float d2 = (tPos.X - pos.X) * (tPos.X - pos.X) + (tPos.Y - pos.Y) * (tPos.Y - pos.Y);

                    // 3. 在范围内的敌人，下发正规的受伤事件 (无硬直)
                    if (d2 <= rSq)
                    {
                        target.AddComponent(EventPool.GetDamageEvent(dmg.Value, causeHitRecovery: false));
                    }
                }
                
                // 确保一发子弹只爆一次，可以移除组件
                bullet.RemoveComponent<AOEComponent>();
            }
        }
        ReturnListToPool(hitEvents);
    }
}