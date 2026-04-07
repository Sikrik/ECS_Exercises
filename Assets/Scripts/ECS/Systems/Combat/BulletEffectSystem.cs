using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 子弹特效系统：处理子弹命中后的扩散逻辑（AOE、闪电链、减速等）
/// 重构点：统一从子弹实体的 DamageComponent 获取伤害源，实现“数据单一真理源”
/// </summary>
public class BulletEffectSystem : SystemBase
{
    public BulletEffectSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 筛选拥有碰撞事件、子弹标记和伤害组件的实体
        var hitBullets = GetEntitiesWith<CollisionEventComponent, BulletTag, DamageComponent>();

        for (int i = hitBullets.Count - 1; i >= 0; i--)
        {
            var bullet = hitBullets[i];
            var evt = bullet.GetComponent<CollisionEventComponent>();
            var dmg = bullet.GetComponent<DamageComponent>(); // 获取统一个伤害组件
            var pos = bullet.GetComponent<PositionComponent>();
            var target = evt.Target;

            if (target == null || !target.IsAlive || !target.HasComponent<EnemyTag>()) continue;

            // 1. 处理减速效果 (挂载 SlowEffectComponent)
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
        
        ReturnListToPool(hitBullets);
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
            if (PoolManager.Instance.SlowVFXPrefab != null)
            {
                GameObject vfxGo = PoolManager.Instance.Spawn(PoolManager.Instance.SlowVFXPrefab, Vector3.zero, Quaternion.identity);
                target.AddComponent(new AttachedVFXComponent(vfxGo));
            }
        }
    }

    private void ProcessAOE(float x, float y, float radius, float damageValue)
    {
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

                if (PoolManager.Instance.LightningChainVFX != null)
                {
                    var nPos = next.GetComponent<PositionComponent>();
                    Vector3 nextPos = new Vector3(nPos.X, nPos.Y, 0);
                    
                    GameObject vfxGo = PoolManager.Instance.Spawn(PoolManager.Instance.LightningChainVFX, Vector3.zero, Quaternion.identity);
                    Entity vfxEntity = ECSManager.Instance.CreateEntity();
                    vfxEntity.AddComponent(new LightningVFXComponent(lastPos, nextPos));
                    vfxEntity.AddComponent(new ViewComponent(vfxGo, PoolManager.Instance.LightningChainVFX));
                    vfxEntity.AddComponent(new LifetimeComponent { Duration = 0.2f });
                    
                    lastPos = nextPos;
                }
                current = next;
            }
            else break;
        }
    }

    private void ApplyDamageEvent(Entity target, float damage)
    {
        var existingEvt = target.GetComponent<DamageTakenEventComponent>();
        if (existingEvt != null)
        {
            existingEvt.DamageAmount += damage;
        }
        else
        {
            target.AddComponent(EventPool.GetDamageEvent(damage));
        }
    }
}