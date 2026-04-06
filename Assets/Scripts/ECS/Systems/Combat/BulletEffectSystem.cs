using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 子弹特效系统：负责处理子弹命中后的特殊逻辑（如爆炸、闪电链、减速）并销毁子弹。
/// </summary>
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

            // 如果目标已死亡或不存在，仅销毁子弹并跳过效果处理
            if (target == null || !target.IsAlive) 
            {
                ecs.DestroyEntity(bullet);
                continue;
            }

            // 1. 处理减速效果挂载
            if (bullet.HasComponent<SlowEffectComponent>())
            {
                var bSlow = bullet.GetComponent<SlowEffectComponent>();
                if (target.HasComponent<SlowEffectComponent>())
                {
                    // 如果目标已有减速效果，则刷新持续时间
                    target.GetComponent<SlowEffectComponent>().RemainingDuration = bSlow.RemainingDuration;
                }
                else
                {
                    // 否则添加减速组件并生成视觉特效
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

            // 4. 关键：子弹命中后统一销毁，防止穿透或重复触发碰撞
            ecs.DestroyEntity(bullet);
        }
    }

    /// <summary>
    /// 处理范围伤害逻辑与特效生成
    /// </summary>
    private void ProcessAOE(float x, float y, AOEComponent aoe)
    {
        var pool = PoolManager.Instance;
        var ecs = ECSManager.Instance;

        // 生成爆炸视觉特效实体
        if (pool.ExplosionVFXPrefab != null)
        {
            GameObject vfxGo = pool.Spawn(pool.ExplosionVFXPrefab, new Vector3(x, y, 0), Quaternion.identity);
            Entity vfxEntity = ecs.CreateEntity();
            vfxEntity.AddComponent(new ViewComponent(vfxGo, pool.ExplosionVFXPrefab));
            vfxEntity.AddComponent(new LifetimeComponent { RemainingTime = 1.0f });
        }

        // 查找范围内敌人并扣除血量
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

    /// <summary>
    /// 处理连锁闪电逻辑与多段路径特效生成
    /// </summary>
    private void ProcessChain(Entity startTarget, PositionComponent hitPos, ChainComponent config)
    {
        var ecs = ECSManager.Instance;
        var pool = PoolManager.Instance;
        
        // 伤害第一个目标
        var startHealth = startTarget.GetComponent<HealthComponent>();
        if (startHealth != null) startHealth.CurrentHealth -= config.Damage;
        
        List<Entity> hitHistory = new List<Entity> { startTarget };
        Entity current = startTarget;
        Vector3 lastPos = new Vector3(hitPos.X, hitPos.Y, 0);

        // 递归寻找后续目标
        for (int i = 0; i < config.MaxTargets - 1; i++)
        {
            Entity next = null;
            float minDistSq = config.Range * config.Range;
            var curPos = current.GetComponent<PositionComponent>();

            var nearby = ecs.Grid.GetNearbyEnemies(curPos.X, curPos.Y);
            foreach (var e in nearby)
            {
                // 跳过已命中的目标以防循环
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

                // 生成闪电链分段 VFX 实体
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