using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 子弹效果系统：响应命中事件，处理逻辑分发和特效生成
/// </summary>
public class BulletEffectSystem : SystemBase
{
    public BulletEffectSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 筛选出本帧发生命中的子弹实体
        var hitBullets = GetEntitiesWith<BulletHitEventComponent, BulletComponent, PositionComponent>();
        var ecs = ECSManager.Instance;

        for (int i = hitBullets.Count - 1; i >= 0; i--)
        {
            var bullet = hitBullets[i];
            var hitEvent = bullet.GetComponent<BulletHitEventComponent>();
            var bulletComp = bullet.GetComponent<BulletComponent>();
            var bulletPos = bullet.GetComponent<PositionComponent>();

            // 1. 处理被命中的首个目标
            if (hitEvent.Target != null && hitEvent.Target.IsAlive)
            {
                ApplyDamage(hitEvent.Target, bulletComp);

                // 2. 触发特殊效果（AOE或闪电链）
                switch (bulletComp.Type)
                {
                    case BulletType.AOE:
                        ApplyAOEEffect(bulletPos.X, bulletPos.Y, ecs.Config);
                        break;
                    case BulletType.ChainLightning:
                        TriggerChainLightning(hitEvent.Target, bulletPos);
                        break;
                }
            }

            // 3. 任务完成，回收子弹实体
            ecs.DestroyEntity(bullet);
        }
    }

    private void ApplyDamage(Entity target, BulletComponent bullet)
    {
        var health = target.GetComponent<HealthComponent>();
        if (health != null) health.CurrentHealth -= bullet.Damage;
    }

    private void ApplyAOEEffect(float x, float y, GameConfig config)
    {
        var enemies = GetEntitiesWith<EnemyComponent, PositionComponent, HealthComponent>();
        float radiusSq = config.AOEBulletRadius * config.AOEBulletRadius;

        foreach (var e in enemies)
        {
            if (!e.IsAlive) continue;
            var p = e.GetComponent<PositionComponent>();
            float d2 = (p.X - x) * (p.X - x) + (p.Y - y) * (p.Y - y);
            if (d2 <= radiusSq)
            {
                e.GetComponent<HealthComponent>().CurrentHealth -= config.AOEBulletDamage;
            }
        }
    }

    /// <summary>
    /// 闪电链弹射逻辑：确保不重复弹射，并使用专门的连线特效
    /// </summary>
    private void TriggerChainLightning(Entity firstTarget, PositionComponent hitPos)
    {
        var ecs = ECSManager.Instance;
        var config = ecs.Config;
        var allEnemies = GetEntitiesWith<EnemyComponent, PositionComponent, HealthComponent>();
        
        // 关键修复：hitHistory 列表防止在两个敌人间无限跳跃
        List<Entity> hitHistory = new List<Entity> { firstTarget };
        Entity currentSource = firstTarget;
        Vector3 lastVfxStart = new Vector3(hitPos.X, hitPos.Y, 0);

        for (int i = 0; i < config.ChainLightningMaxTargets - 1; i++)
        {
            Entity nextTarget = null;
            float minDistSq = config.ChainLightningChainRange * config.ChainLightningChainRange;
            var curPos = currentSource.GetComponent<PositionComponent>();

            // 寻找范围内且未被电击过的最近目标
            foreach (var e in allEnemies)
            {
                if (!e.IsAlive || hitHistory.Contains(e)) continue;
                
                var ePos = e.GetComponent<PositionComponent>();
                float d2 = (ePos.X - curPos.X) * (ePos.X - curPos.X) + (ePos.Y - curPos.Y) * (ePos.Y - curPos.Y);
                if (d2 < minDistSq)
                {
                    minDistSq = d2;
                    nextTarget = e;
                }
            }

            if (nextTarget != null)
            {
                // 1. 逻辑效果
                hitHistory.Add(nextTarget);
                nextTarget.GetComponent<HealthComponent>().CurrentHealth -= config.ChainLightningDamage;
                
                // 2. 表现效果：创建 VFX 实体
                var nextPos = nextTarget.GetComponent<PositionComponent>();
                Entity vfxEntity = ecs.CreateEntity();
                vfxEntity.AddComponent(new LightningVFXComponent(lastVfxStart, new Vector3(nextPos.X, nextPos.Y, 0)));
                
                // 关键修复：从 PoolManager 申请专门的“连线预制体”，不再申请“子弹预制体”
                GameObject vfxGo = PoolManager.Instance.Spawn(PoolManager.Instance.LightningChainVFX, Vector3.zero, Quaternion.identity);
                vfxEntity.AddComponent(new ViewComponent(vfxGo));

                // 3. 更新弹射源位置
                currentSource = nextTarget;
                lastVfxStart = new Vector3(nextPos.X, nextPos.Y, 0);
            }
            else break; // 范围内没有目标了，跳出循环
        }
    }
}