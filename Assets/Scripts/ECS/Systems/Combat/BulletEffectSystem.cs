using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 子弹特效系统：负责处理命中的后续物理扩散逻辑（AOE、闪电链、减速）
/// 优化点：不再直接扣血，统一发放 DamageTakenEvent 事件
/// </summary>
public class BulletEffectSystem : SystemBase
{
    public BulletEffectSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 抓取本帧发生了碰撞的子弹
        var hitBullets = GetEntitiesWith<CollisionEventComponent, BulletTag>();

        for (int i = hitBullets.Count - 1; i >= 0; i--)
        {
            var bullet = hitBullets[i];
            var evt = bullet.GetComponent<CollisionEventComponent>();
            var pos = bullet.GetComponent<PositionComponent>();
            var target = evt.Target;

            // 阵营校验：仅对存活的敌人触发效果
            if (target == null || !target.IsAlive || !target.HasComponent<EnemyTag>()) continue;

            // 1. 处理减速效果挂载 (仅逻辑与表现，不涉及数值计算)
            HandleSlowEffect(bullet, target);

            // 2. 处理爆炸范围伤害 (AOE) -> 发射伤害意图
            var aoe = bullet.GetComponent<AOEComponent>();
            if (aoe != null) ProcessAOE(pos.X, pos.Y, aoe);

            // 3. 处理连锁闪电 -> 递归寻找目标并持续发射伤害意图
            var chain = bullet.GetComponent<ChainComponent>();
            if (chain != null) ProcessChain(target, pos, chain);

            // 4. 标记销毁
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
            tSlow.Duration = bSlow.Duration; // 刷新持续时间
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

    private void ProcessAOE(float x, float y, AOEComponent aoe)
    {
        var enemies = ECSManager.Instance.Grid.GetNearbyEnemies(x, y);
        float rSq = aoe.Radius * aoe.Radius;
    
        foreach (var e in enemies)
        {
            if (!e.IsAlive || !e.HasComponent<EnemyTag>()) continue;
        
            var p = e.GetComponent<PositionComponent>();
            float d2 = (p.X - x) * (p.X - x) + (p.Y - y) * (p.Y - y);
        
            if (d2 <= rSq)
            {
                ApplyDamageEvent(e, aoe.Damage); // 仅产生伤害意图
            }
        }
    }

    private void ProcessChain(Entity startTarget, PositionComponent hitPos, ChainComponent config)
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
                ApplyDamageEvent(next, config.Damage); // 仅产生伤害意图

                // 表现层：生成闪电链 VFX
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
            existingEvt.DamageAmount += damage; // 累加伤害意图
        }
        else
        {
            target.AddComponent(EventPool.GetDamageEvent(damage)); // 从池中获取事件组件
        }
    }
}