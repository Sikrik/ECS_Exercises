using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 子弹特效系统：处理子弹命中后的逻辑运算（减速、范围伤害、闪电链等）
/// 【高内聚改造版】：绝对不触碰 GameObject_PoolManager，改用单帧事件解耦视觉渲染。
/// </summary>
public class BulletEffectSystem : SystemBase
{
    public BulletEffectSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var hitEvents = GetEntitiesWith<CollisionEventComponent>();

        for (int i = hitEvents.Count - 1; i >= 0; i--)
        {
            var evtEntity = hitEvents[i];
            var evt = evtEntity.GetComponent<CollisionEventComponent>();
            var bullet = evt.Source; 
            var target = evt.Target; 

            if (bullet == null || !bullet.IsAlive || !bullet.HasComponent<BulletTag>()) continue;
            if (target == null || !target.IsAlive || !target.HasComponent<EnemyTag>()) continue;

            var dmg = bullet.GetComponent<DamageComponent>(); 
            var pos = bullet.GetComponent<PositionComponent>();

            // 1. 处理减速效果
            HandleSlowEffect(bullet, target);

            // 2. 处理爆炸范围伤害 (AOE)
            var aoe = bullet.GetComponent<AOEComponent>();
            if (aoe != null) 
            {
                ProcessAOE(pos.X, pos.Y, aoe.Radius, dmg.Value);
            }

            // 3. 处理连锁闪电
            var chain = bullet.GetComponent<ChainComponent>();
            if (chain != null) 
            {
                ProcessChain(target, pos, chain, dmg.Value);
            }

            // 4. 任务完成，标记子弹销毁
            if (!bullet.HasComponent<PendingDestroyComponent>())
                bullet.AddComponent(new PendingDestroyComponent());
        }
        
        ReturnListToPool(hitEvents);
    }

    private void HandleSlowEffect(Entity bullet, Entity target)
    {
        var bSlow = bullet.GetComponent<SlowEffectComponent>();
        if (bSlow == null) return;

        var tSlow = target.GetComponent<SlowEffectComponent>();
        if (tSlow != null)
        {
            tSlow.Duration = bSlow.Duration; // 刷新时间
        }
        else
        {
            target.AddComponent(new SlowEffectComponent(bSlow.SlowRatio, bSlow.Duration));
            
            // 【下发特效意图】：通知表现层给该目标挂载减速特效
            Entity vfxEvent = ECSManager.Instance.CreateEntity();
            vfxEvent.AddComponent(new VFXSpawnEventComponent { 
                VFXType = "SlowVFX", 
                AttachTarget = target 
            });
        }
    }

    private void ProcessAOE(float x, float y, float radius, float damageValue)
    {
        // 【下发特效意图】：通知表现层在指定坐标播放爆炸
        Entity vfxEvent = ECSManager.Instance.CreateEntity();
        vfxEvent.AddComponent(new VFXSpawnEventComponent { 
            VFXType = "Explosion", 
            Position = new Vector3(x, y, 0) 
        });

        var enemies = ECSManager.Instance.Grid.GetNearbyEnemies(x, y);
        float rSq = radius * radius;

        foreach (var e in enemies)
        {
            if (!e.IsAlive || !e.HasComponent<EnemyTag>()) continue;
    
            var p = e.GetComponent<PositionComponent>();
            float d2 = (p.X - x) * (p.X - x) + (p.Y - y) * (p.Y - y);
    
            if (d2 <= rSq)
            {
                ApplyDamageEvent(e, damageValue); 
            }
        }
    }

    private void ProcessChain(Entity startTarget, PositionComponent hitPos, ChainComponent config, float damageValue)
    {
        List<Entity> hitHistory = new List<Entity> { startTarget };
        Entity current = startTarget;
        Vector3 lastPos = new Vector3(hitPos.X, hitPos.Y, 0);

        for (int i = 0; i < config.MaxTargets - 1; i++)
        {
            Entity next = null;
            float minDistSq = config.Range * config.Range;
            var curPos = current.GetComponent<PositionComponent>();

            var nearby = ECSManager.Instance.Grid.GetNearbyEnemies(curPos.X, curPos.Y);
            foreach (var e in nearby)
            {
                if (!e.IsAlive || !e.HasComponent<EnemyTag>() || hitHistory.Contains(e)) continue;
                
                var ePos = e.GetComponent<PositionComponent>();
                float d2 = (ePos.X - curPos.X) * (ePos.X - curPos.X) + (ePos.Y - curPos.Y) * (ePos.Y - curPos.Y);
                if (d2 < minDistSq) { minDistSq = d2; next = e; }
            }

            if (next != null)
            {
                hitHistory.Add(next);
                ApplyDamageEvent(next, damageValue);

                var nPos = next.GetComponent<PositionComponent>();
                Vector3 nextPos = new Vector3(nPos.X, nPos.Y, 0);
                
                // 【下发特效意图】：通知表现层绘制闪电链
                Entity vfxEvent = ECSManager.Instance.CreateEntity();
                vfxEvent.AddComponent(new VFXSpawnEventComponent { 
                    VFXType = "LightningChain", 
                    Position = lastPos,
                    EndPosition = nextPos 
                });
                
                lastPos = nextPos;
                current = next;
            }
            else break;
        }
    }

    private void ApplyDamageEvent(Entity target, float damage)
    {
        var hp = target.GetComponent<HealthComponent>();
        if (hp != null)
        {
            hp.CurrentHealth -= damage;
        }
    }
}