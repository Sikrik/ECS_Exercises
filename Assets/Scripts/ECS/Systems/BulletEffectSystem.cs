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

            // 目标死亡或不存在则跳过
            if (target == null || !target.IsAlive) continue;

            // 1. 处理基础伤害
            if (bullet.HasComponent<DamageComponent>())
            {
                ApplyDamage(target, bullet.GetComponent<DamageComponent>().Value);
            }

            // 2. 处理爆炸 (AOE)
            if (bullet.HasComponent<AOEComponent>())
            {
                ProcessAOE(pos.X, pos.Y, bullet.GetComponent<AOEComponent>());
            }

            // 3. 处理闪电链
            if (bullet.HasComponent<ChainComponent>())
            {
                ProcessChain(target, pos, bullet.GetComponent<ChainComponent>());
            }

            // 4. 处理减速效果挂载
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
                    
                    // 挂载跟随特效
                    var slowVfxPrefab = PoolManager.Instance.SlowVFXPrefab;
                    if (slowVfxPrefab != null)
                    {
                        GameObject vfxGo = PoolManager.Instance.Spawn(slowVfxPrefab, Vector3.zero, Quaternion.identity);
                        target.AddComponent(new AttachedVFXComponent(vfxGo));
                    }
                }
            }

            // 完成处理，销毁子弹实体（内部会自动回收 GameObject）
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
        var ecs = ECSManager.Instance;

        // 生成爆炸特效实体
        if (pool.ExplosionVFXPrefab != null)
        {
            GameObject vfxGo = pool.Spawn(pool.ExplosionVFXPrefab, new Vector3(x, y, 0), Quaternion.identity);
            Entity vfxEntity = ecs.CreateEntity();
            vfxEntity.AddComponent(new ViewComponent(vfxGo, pool.ExplosionVFXPrefab));
            vfxEntity.AddComponent(new LifetimeComponent { RemainingTime = 1.0f }); // 1秒后自动销毁
        }

        // 范围伤害计算
        var enemies = ecs.Grid.GetNearbyEnemies(x, y);
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
        var pool = PoolManager.Instance;
        
        // 伤害第一个目标
        ApplyDamage(startTarget, config.Damage);
        
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
                ApplyDamage(next, config.Damage);
                var nPos = next.GetComponent<PositionComponent>();
                Vector3 nextPos = new Vector3(nPos.X, nPos.Y, 0);

                // 生成闪电链线段特效实体
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