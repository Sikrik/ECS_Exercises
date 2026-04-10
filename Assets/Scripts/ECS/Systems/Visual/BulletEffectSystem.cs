using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 子弹特效系统：处理子弹命中后的扩散逻辑（AOE、闪电链、减速等）
/// </summary>
public class BulletEffectSystem : SystemBase
{
    public BulletEffectSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 【核心修改】：改为查询所有独立的碰撞事件实体
        var hitEvents = GetEntitiesWith<CollisionEventComponent>();

        for (int i = hitEvents.Count - 1; i >= 0; i--)
        {
            var evtEntity = hitEvents[i];
            var evt = evtEntity.GetComponent<CollisionEventComponent>();
            var bullet = evt.Source; // 提取源头
            var target = evt.Target; // 提取目标

            // 新增安全校验：确保源头是活着的子弹，目标是活着的敌人
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
            if (GameObject_PoolManager.Instance.SlowVFXPrefab != null)
            {
                GameObject vfxGo = GameObject_PoolManager.Instance.Spawn(GameObject_PoolManager.Instance.SlowVFXPrefab, Vector3.zero, Quaternion.identity);
                target.AddComponent(new AttachedVFXComponent(vfxGo));
            }
        }
    }

    private void ProcessAOE(float x, float y, float radius, float damageValue)
    {
        if (GameObject_PoolManager.Instance.ExplosionVFXPrefab != null)
        {
            GameObject vfxGo = GameObject_PoolManager.Instance.Spawn(
                GameObject_PoolManager.Instance.ExplosionVFXPrefab, 
                new Vector3(x, y, 0), 
                Quaternion.identity
            );

            Entity vfxEntity = ECSManager.Instance.CreateEntity();
            vfxEntity.AddComponent(new ViewComponent(vfxGo, GameObject_PoolManager.Instance.ExplosionVFXPrefab));
            vfxEntity.AddComponent(new LifetimeComponent { Duration = 0.5f }); 
        }

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

                if (GameObject_PoolManager.Instance.LightningChainVFX != null)
                {
                    var nPos = next.GetComponent<PositionComponent>();
                    Vector3 nextPos = new Vector3(nPos.X, nPos.Y, 0);
                    
                    GameObject vfxGo = GameObject_PoolManager.Instance.Spawn(GameObject_PoolManager.Instance.LightningChainVFX, Vector3.zero, Quaternion.identity);
                    Entity vfxEntity = ECSManager.Instance.CreateEntity();
                    vfxEntity.AddComponent(new LightningVFXComponent(lastPos, nextPos));
                    vfxEntity.AddComponent(new ViewComponent(vfxGo, GameObject_PoolManager.Instance.LightningChainVFX));
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
        var hp = target.GetComponent<HealthComponent>();
        if (hp != null)
        {
            hp.CurrentHealth -= damage;
        }
    }
}