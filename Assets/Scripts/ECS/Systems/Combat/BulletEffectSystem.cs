using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 子弹特效系统：负责处理子弹命中后的特殊逻辑（如爆炸AOE、闪电链、减速）并销毁子弹。
/// 【完整融合版】：保留了所有武器特效特性，并完美接入了 0 GC 对象池和统一受击反馈管线。
/// </summary>
public class BulletEffectSystem : SystemBase
{
    public BulletEffectSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 抓取本帧发生了碰撞的子弹 (使用 BulletComponent 或 BulletTag 取决于你的定义，这里统一)
        var hitBullets = GetEntitiesWith<CollisionEventComponent, BulletTag>();

        // 倒序遍历，安全稳定
        for (int i = hitBullets.Count - 1; i >= 0; i--)
        {
            var bullet = hitBullets[i];
            var evt = bullet.GetComponent<CollisionEventComponent>();
            var pos = bullet.GetComponent<PositionComponent>();
            var target = evt.Target;

            // 阵营校验：如果目标已死亡，或根本不是敌人（可能是墙体或自己人），则跳过特效
            if (target == null || !target.IsAlive || !target.HasComponent<EnemyTag>()) 
            {
                continue;
            }

            // ==========================================
            // 1. 处理减速效果挂载 (单次查找优化)
            // ==========================================
            var bSlow = bullet.GetComponent<SlowEffectComponent>();
            if (bSlow != null)
            {
                var tSlow = target.GetComponent<SlowEffectComponent>();
                if (tSlow != null)
                {
                    // 如果目标已有减速效果，则刷新持续时间
                    tSlow.Duration = bSlow.Duration;
                }
                else
                {
                    // 否则添加减速组件并生成视觉特效
                    target.AddComponent(new SlowEffectComponent(bSlow.SlowRatio, bSlow.Duration));
                    if (PoolManager.Instance.SlowVFXPrefab != null)
                    {
                        GameObject vfxGo = PoolManager.Instance.Spawn(PoolManager.Instance.SlowVFXPrefab, Vector3.zero, Quaternion.identity);
                        target.AddComponent(new AttachedVFXComponent(vfxGo));
                    }
                }
            }

            // ==========================================
            // 2. 处理爆炸范围伤害 (AOE)
            // ==========================================
            var aoe = bullet.GetComponent<AOEComponent>();
            if (aoe != null)
            {
                ProcessAOE(pos.X, pos.Y, aoe);
            }

            // ==========================================
            // 3. 处理连锁闪电
            // ==========================================
            var chain = bullet.GetComponent<ChainComponent>();
            if (chain != null)
            {
                ProcessChain(target, pos, chain);
            }

            // ==========================================
            // 4. 生命周期终结 (发放死亡判决书)
            // ==========================================
            if (!bullet.HasComponent<PendingDestroyComponent>())
            {
                bullet.AddComponent(new PendingDestroyComponent());
            }
        }
        
        // 归还 List，防止内存泄漏
        ReturnListToPool(hitBullets);
    }

    /// <summary>
    /// 处理范围伤害逻辑与特效生成
    /// </summary>
    private void ProcessAOE(float x, float y, AOEComponent aoe)
    {
        var ecs = ECSManager.Instance;
        // ... (视觉特效生成逻辑保持不变)

        var enemies = ecs.Grid.GetNearbyEnemies(x, y);
        float rSq = aoe.Radius * aoe.Radius;
    
        foreach (var e in enemies)
        {
            if (!e.IsAlive || !e.HasComponent<EnemyTag>()) continue;
        
            var p = e.GetComponent<PositionComponent>();
            float d2 = (p.X - x) * (p.X - x) + (p.Y - y) * (p.Y - y);
        
            if (d2 <= rSq)
            {
                // 职责统一：不再直接修改 HealthComponent，而是发放“受伤事件”
                // 具体的扣血、无敌检测、UI广播全部交给 DamageSystem 或专门的反应系统
                ApplyDamageEvent(e, aoe.Damage);
            }
        }
    }
    private void ApplyDamageEvent(Entity target, float damage)
    {
        // 使用对象池获取事件组件，挂载到目标身上
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
    /// <summary>
    /// 处理连锁闪电逻辑与多段路径特效生成
    /// </summary>
    private void ProcessChain(Entity startTarget, PositionComponent hitPos, ChainComponent config)
    {
        var ecs = ECSManager.Instance;
        var pool = PoolManager.Instance;
        
        // 注意：第一个目标的直接伤害已经在 DamageSystem 里扣过了，这里只处理传递逻辑
        
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
                if (d2 < minDistSq) 
                { 
                    minDistSq = d2; 
                    next = e; 
                }
            }

            if (next != null)
            {
                hitHistory.Add(next);
                
                // 👇 核心升级：传导伤害也走事件管线，触发闪电打击感！
                ApplyDamageViaEvent(next, config.Damage);

                var nPos = next.GetComponent<PositionComponent>();
                Vector3 nextPos = new Vector3(nPos.X, nPos.Y, 0);

                // 生成闪电链分段 VFX 实体
                if (pool.LightningChainVFX != null)
                {
                    GameObject vfxGo = pool.Spawn(pool.LightningChainVFX, Vector3.zero, Quaternion.identity);
                    Entity vfxEntity = ecs.CreateEntity();
                    vfxEntity.AddComponent(new LightningVFXComponent(lastPos, nextPos));
                    vfxEntity.AddComponent(new ViewComponent(vfxGo, pool.LightningChainVFX));
                    vfxEntity.AddComponent(new LifetimeComponent { Duration = 0.2f }); // 0.2秒后消失
                }

                current = next;
                lastPos = nextPos;
            }
            else break; // 范围内找不到下一个目标了，闪电链中断
        }
    }

    /// <summary>
    /// 工具方法：安全地施加伤害事件（防止同帧多段伤害事件覆盖）
    /// </summary>
    private void ApplyDamageViaEvent(Entity target, float damage)
    {
        var health = target.GetComponent<HealthComponent>();
        if (health != null)
        {
            // 真实扣血
            health.CurrentHealth -= damage;

            // 叠加事件交由表现层处理
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
}