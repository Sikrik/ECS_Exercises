using System.Collections.Generic;
using UnityEngine;

public class BulletEffectSystem : SystemBase
{
    public BulletEffectSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 筛选：发生了碰撞 且 确定身份是子弹 的实体
        var hitBullets = GetEntitiesWith<CollisionEventComponent, BulletTag>();
        var ecs = ECSManager.Instance;

        foreach (var bullet in hitBullets)
        {
            var evt = bullet.GetComponent<CollisionEventComponent>();
            var pos = bullet.GetComponent<PositionComponent>();
            var target = evt.Target;

            if (target == null || !target.IsAlive) continue;

            // 1. 处理减速效果
            if (bullet.HasComponent<SlowEffectComponent>())
            {
                var bSlow = bullet.GetComponent<SlowEffectComponent>();
                if (target.HasComponent<SlowEffectComponent>())
                    target.GetComponent<SlowEffectComponent>().RemainingDuration = bSlow.RemainingDuration;
                else
                {
                    target.AddComponent(new SlowEffectComponent(bSlow.SlowRatio, bSlow.RemainingDuration));
                    if (PoolManager.Instance.SlowVFXPrefab != null)
                    {
                        GameObject vfxGo = PoolManager.Instance.Spawn(PoolManager.Instance.SlowVFXPrefab, Vector3.zero, Quaternion.identity);
                        target.AddComponent(new AttachedVFXComponent(vfxGo));
                    }
                }
            }

            // 2. 处理爆炸范围伤害 (AOE)
            if (bullet.HasComponent<AOEComponent>())
                ProcessAOE(pos.X, pos.Y, bullet.GetComponent<AOEComponent>());

            // 3. 处理连锁闪电
            if (bullet.HasComponent<ChainComponent>())
                ProcessChain(target, pos, bullet.GetComponent<ChainComponent>());

            // 4. 关键：子弹完成使命，统一销毁（防止一弹多穿造成的秒杀）
            ecs.DestroyEntity(bullet);
        }
    }

    // AOE 和 闪电链的具体逻辑复用之前的实现
    private void ProcessAOE(float x, float y, AOEComponent aoe)
    {
        var pool = PoolManager.Instance;
        var ecs = ECSManager.Instance;

        if (pool.ExplosionVFXPrefab != null)
        {
            GameObject vfxGo = pool.Spawn(pool.ExplosionVFXPrefab, new Vector3(x, y, 0), Quaternion.identity);
            Entity vfxEntity = ecs.CreateEntity();
            vfxEntity.AddComponent(new ViewComponent(vfxGo, pool.ExplosionVFXPrefab));
            vfxEntity.AddComponent(new LifetimeComponent { RemainingTime = 1.0f });
        }

        var enemies = ecs.Grid.GetNearbyEnemies(x, y);
        float rSq = aoe.Radius * aoe.Radius;
        foreach (var e in enemies)
        {
            if (!e.IsAlive || !e.HasComponent<EnemyTag>()) continue;
            var p = e.GetComponent<PositionComponent>();
            float d2 = (p.X - x) * (p.X - x) + (p.Y - y) * (p.Y - y);
            if (d2 <= rSq) 
            {
                var health = e.GetComponent<HealthComponent>();
                if (health != null) health.CurrentHealth -= aoe.Damage;
            }
        }
    }

    private void ProcessChain(Entity startTarget, PositionComponent hitPos, ChainComponent config)
    {
        var ecs = ECSManager.Instance;
        var pool = PoolManager.Instance;
        
        var startHealth = startTarget.GetComponent<HealthComponent>();
        if (startHealth != null) startHealth.CurrentHealth -= config.Damage;
        
        List<Entity> hitHistory = new List<Entity> { startTarget };
        Entity current = startTarget;
        Vector3 lastPos = new Vector3(hitPos.X, hitPos.Y, 0);

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
                var nextHealth = next.GetComponent<HealthComponent>();
                if (nextHealth != null) nextHealth.CurrentHealth -= config.Damage;

                var nPos = next.GetComponent<PositionComponent>();
                Vector3 nextPos = new Vector3(nPos.X, nPos.Y, 0);

                if (pool.LightningChainVFX != null)
                {
                    GameObject vfxGo = pool.Spawn(pool.LightningChainVFX, Vector3.zero, Quaternion.identity);
                    Entity vfxEntity = ecs.CreateEntity();
                    vfxEntity.AddComponent(new LightningVFXComponent(lastPos, nextPos));
                    vfxEntity.AddComponent(new ViewComponent(vfxGo, pool.LightningChainVFX));
                    vfxEntity.AddComponent(new LifetimeComponent { RemainingTime = 0.2f });
                }

                current = next;
                lastPos = nextPos;
            }
            else break;
        }
    }
}