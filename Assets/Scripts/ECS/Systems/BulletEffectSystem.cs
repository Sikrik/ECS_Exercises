using System.Collections.Generic;
using UnityEngine;

public class BulletEffectSystem : SystemBase
{
    public BulletEffectSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var hitBullets = GetEntitiesWith<BulletHitEventComponent, PositionComponent>();
        var ecs = ECSManager.Instance;

        foreach (var bullet in hitBullets)
        {
            var hitEvent = bullet.GetComponent<BulletHitEventComponent>();
            var pos = bullet.GetComponent<PositionComponent>();
            var target = hitEvent.Target;

            if (target == null || !target.IsAlive) continue;

            // 1. 处理单体伤害 (Normal, Slow)
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
                    // 挂载冰冻特效视觉组件
                    if (PoolManager.Instance.SlowVFXPrefab != null)
                    {
                        GameObject vfx = PoolManager.Instance.Spawn(PoolManager.Instance.SlowVFXPrefab, Vector3.zero, Quaternion.identity);
                        target.AddComponent(new AttachedVFXComponent(vfx));
                    }
                }
            }

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
        // 视觉：生成爆炸特效
        if (PoolManager.Instance.ExplosionVFXPrefab != null)
        {
            PoolManager.Instance.Spawn(PoolManager.Instance.ExplosionVFXPrefab, new Vector3(x, y, 0), Quaternion.identity);
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
        
        // --- 核心修复：第一个目标也需要受到伤害 ---
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

                // 生成闪电链视觉实体
                Entity vfx = ecs.CreateEntity();
                vfx.AddComponent(new LightningVFXComponent(lastVfxPos, nextPosV3));
                if (PoolManager.Instance.LightningChainVFX != null)
                {
                    vfx.AddComponent(new ViewComponent(PoolManager.Instance.Spawn(PoolManager.Instance.LightningChainVFX, Vector3.zero, Quaternion.identity)));
                }

                current = next;
                lastVfxPos = nextPosV3;
            }
            else break;
        }
    }
}