using System.Collections.Generic;
using UnityEngine;

public class BulletEffectSystem : SystemBase
{
    public BulletEffectSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 获取所有发生了碰撞事件的子弹
        var hitBullets = GetEntitiesWith<BulletHitEventComponent, PositionComponent>();
        var ecs = ECSManager.Instance;

        foreach (var bullet in hitBullets)
        {
            var hitEvent = bullet.GetComponent<BulletHitEventComponent>();
            var pos = bullet.GetComponent<PositionComponent>();
            var target = hitEvent.Target;

            if (target == null || !target.IsAlive) continue;

            // 1. 处理单体伤害
            if (bullet.HasComponent<DamageComponent>())
            {
                var dmg = bullet.GetComponent<DamageComponent>().Value;
                ApplyDamage(target, dmg);
            }

            // 2. 处理爆炸效果 (AOE)
            if (bullet.HasComponent<AOEComponent>())
            {
                var aoe = bullet.GetComponent<AOEComponent>();
                ProcessAOE(pos.X, pos.Y, aoe);
            }

            // 3. 处理闪电链弹射
            if (bullet.HasComponent<ChainComponent>())
            {
                var chain = bullet.GetComponent<ChainComponent>();
                ProcessChain(target, pos, chain);
            }

            // 4. 处理减速状态挂载
            if (bullet.HasComponent<SlowEffectComponent>())
            {
                var bSlow = bullet.GetComponent<SlowEffectComponent>();
                if (target.HasComponent<SlowEffectComponent>())
                {
                    target.GetComponent<SlowEffectComponent>().RemainingDuration = bSlow.RemainingDuration;
                }
                else
                {
                    target.AddComponent(new SlowEffectComponent(bSlow.SlowRatio, bSlow.RemainingDuration));
                    
                    // 挂载冰冻特效视觉组件 (此处由 SlowEffectSystem 负责回收，不加 Lifetime)
                    var slowPrefab = PoolManager.Instance.SlowVFXPrefab;
                    if (slowPrefab != null)
                    {
                        GameObject vfx = PoolManager.Instance.Spawn(slowPrefab, Vector3.zero, Quaternion.identity);
                        target.AddComponent(new AttachedVFXComponent(vfx));
                    }
                }
            }

            // 销毁子弹实体
            ecs.DestroyEntity(bullet);
        }
    }

    private void ApplyDamage(Entity target, float damage)
    {
        var health = target.GetComponent<HealthComponent>();
        if (health != null) health.CurrentHealth -= damage;
    }

    private void ProcessAOE(float x, float y, AOEComponent aoe)
    {
        var pool = PoolManager.Instance;
        var explosionPrefab = pool.ExplosionVFXPrefab;

        // --- 核心更新：使用 ECS 托管爆炸特效寿命 ---
        if (explosionPrefab != null)
        {
            GameObject vfxGo = pool.Spawn(explosionPrefab, new Vector3(x, y, 0), Quaternion.identity);
            Entity vfxEntity = ECSManager.Instance.CreateEntity();
            // 传入实例和预制体以便对象池回收
            vfxEntity.AddComponent(new ViewComponent(vfxGo, explosionPrefab)); 
            // 设置 1 秒后自动销毁
            vfxEntity.AddComponent(new LifetimeComponent { RemainingTime = 1.0f }); 
        }

        var enemies = ECSManager.Instance.Grid.GetNearbyEnemies(x, y);
        float rSq = aoe.Radius * aoe.Radius;
        foreach (var e in enemies)
        {
            if (!e.IsAlive || !e.HasComponent<EnemyTag>()) continue;
            var p = e.GetComponent<PositionComponent>();
            float d2 = (p.X - x) * (p.X - x) + (p.Y - y) * (p.Y - y);
            if (d2 <= rSq) ApplyDamage(e, aoe.Damage);
        }
    }

    private void ProcessChain(Entity startTarget, PositionComponent hitPos, ChainComponent config)
    {
        var ecs = ECSManager.Instance;
        ApplyDamage(startTarget, config.Damage);
        
        List<Entity> hitHistory = new List<Entity> { startTarget };
        Entity current = startTarget;
        Vector3 lastVfxPos = new Vector3(hitPos.X, hitPos.Y, 0);

        for (int i = 0; i < config.MaxTargets - 1; i++)
        {
            Entity next = null;
            float minDistSq = config.Range * config.Range;
            var curPos = current.GetComponent<PositionComponent>();

            var nearby = ecs.Grid.GetNearbyEnemies(curPos.X, curPos.Y);
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
                ApplyDamage(next, config.Damage);
                var nPos = next.GetComponent<PositionComponent>();
                Vector3 nextPosV3 = new Vector3(nPos.X, nPos.Y, 0);

                // --- 核心更新：生成闪电链视觉实体并设置寿命 ---
                var chainPrefab = PoolManager.Instance.LightningChainVFX;
                if (chainPrefab != null)
                {
                    GameObject vfxGo = PoolManager.Instance.Spawn(chainPrefab, Vector3.zero, Quaternion.identity);
                    Entity vfxEntity = ecs.CreateEntity();
                    vfxEntity.AddComponent(new LightningVFXComponent(lastVfxPos, nextPosV3));
                    vfxEntity.AddComponent(new ViewComponent(vfxGo, chainPrefab));
                    vfxEntity.AddComponent(new LifetimeComponent { RemainingTime = 0.2f }); // 闪电链存在时间较短
                }

                current = next;
                lastVfxPos = nextPosV3;
            }
            else break;
        }
    }
}