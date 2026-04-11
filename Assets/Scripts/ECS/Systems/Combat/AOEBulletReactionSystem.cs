using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 范围爆炸弹反应系统（原子化）
/// 修复：增加了严格的碰撞目标身份与阵营校验，防止子弹在枪口（玩家中心）原地引爆。
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
            var target = evt.Target; // 获取碰撞目标
            
            // 1. 校验子弹自身的合法性
            if (bullet == null || !bullet.IsAlive || !bullet.HasComponent<AOEComponent>()) continue;
            
            // 2. 【核心修复】校验碰撞目标的合法性（必须是活着的敌人）
            // 如果子弹出膛时碰到了玩家自己，或者碰到了已经死亡的残骸，直接跳过，不引爆
            if (target == null || !target.IsAlive || !target.HasComponent<EnemyTag>()) continue;

            // 【进阶做法：如果你未来要加会发射 AOE 的怪物，这里可以改为基于 FactionComponent 的阵营过滤】
            // var bFaction = bullet.GetComponent<FactionComponent>();
            // var tFaction = target.GetComponent<FactionComponent>();
            // if (bFaction != null && tFaction != null && bFaction.Value == tFaction.Value) continue;
            
            // 为了防止同一发 AOE 穿透时连续触发爆炸，要求子弹必须是第一次命中或正在销毁
            if (bullet.HasComponent<PendingDestroyComponent>() || !bullet.HasComponent<PierceComponent>())
            {
                var aoe = bullet.GetComponent<AOEComponent>();
                var dmg = bullet.GetComponent<DamageComponent>();
                
                // 获取此时子弹的坐标（即命中的发生点）
                var pos = bullet.GetComponent<PositionComponent>();

                // 1. 触发爆炸特效意图
                // 此时的 pos.X 和 pos.Y 是准确的击中敌人时的坐标，特效不会再偏移到玩家身上
                Entity vfxEvent = ECSManager.Instance.CreateEntity();
                vfxEvent.AddComponent(new VFXSpawnEventComponent { 
                    VFXType = "Explosion", 
                    Position = new Vector3(pos.X, pos.Y, 0) 
                });

                // 2. 利用 GridSystem 查找爆炸中心范围内的所有敌人
                var enemies = ECSManager.Instance.Grid.GetNearbyEnemies(pos.X, pos.Y);
                float rSq = aoe.Radius * aoe.Radius;

                foreach (var e in enemies)
                {
                    if (!e.IsAlive || !e.HasComponent<EnemyTag>()) continue;

                    var tPos = e.GetComponent<PositionComponent>();
                    float d2 = (tPos.X - pos.X) * (tPos.X - pos.X) + (tPos.Y - pos.Y) * (tPos.Y - pos.Y);

                    // 3. 在范围内的敌人，下发正规的受伤事件 (AOE 通常不附带单体的高强度物理硬直)
                    if (d2 <= rSq)
                    {
                        e.AddComponent(EventPool.GetDamageEvent(dmg.Value, causeHitRecovery: false));
                    }
                }
                
                // 确保一发子弹只爆一次，完成溅射后移除范围组件
                bullet.RemoveComponent<AOEComponent>();
            }
        }
        
        // 维持 0 GC
        ReturnListToPool(hitEvents);
    }
}