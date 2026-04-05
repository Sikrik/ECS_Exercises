using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 子弹碰撞系统：处理子弹与敌人的碰撞检测、伤害应用及特殊效果（AOE、闪电链等）
/// </summary>
public class BulletCollisionSystem : SystemBase
{
    public BulletCollisionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        CheckBulletEnemyCollision();
    }

    private void CheckBulletEnemyCollision()
    {
        // 筛选带有碰撞和位置组件的子弹与敌人
        var bullets = GetEntitiesWith<BulletComponent, PositionComponent, CollisionComponent>();
        var enemies = GetEntitiesWith<EnemyComponent, PositionComponent, CollisionComponent, HealthComponent>();
        
        var ecs = ECSManager.Instance;
        var config = ecs.Config;

        // 倒序遍历，确保销毁操作安全
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            var bullet = bullets[i];
            if (!bullet.IsAlive) continue;

            var bulletPos = bullet.GetComponent<PositionComponent>();
            var bulletCol = bullet.GetComponent<CollisionComponent>();
            var bulletComp = bullet.GetComponent<BulletComponent>();

            for (int j = enemies.Count - 1; j >= 0; j--)
            {
                var enemy = enemies[j];
                if (!enemy.IsAlive) continue;

                var enemyPos = enemy.GetComponent<PositionComponent>();
                var enemyCol = enemy.GetComponent<CollisionComponent>();
                
                // 计算双方碰撞半径之和
                float combinedRadius = bulletCol.Radius + enemyCol.Radius;

                // 核心修复：线段与圆碰撞检测，解决高速子弹穿透问题（隧道效应）
                if (CheckSegmentCircleIntersection(
                    bulletPos.PreviousX, bulletPos.PreviousY, 
                    bulletPos.X, bulletPos.Y, 
                    enemyPos.X, enemyPos.Y, combinedRadius))
                {
                    // 触发碰撞逻辑
                    HandleCollision(bullet, enemy, ecs, config);
                    // 子弹命中后销毁，跳出当前敌人的循环
                    break; 
                }
            }
        }
    }

    /// <summary>
    /// 线段与圆碰撞检测：检测子弹上帧到本帧的位移线段是否穿过敌人的圆
    /// </summary>
    private bool CheckSegmentCircleIntersection(float p1x, float p1y, float p2x, float p2y, float cx, float cy, float r)
    {
        float dx = p2x - p1x;
        float dy = p2y - p1y;
        float lineLenSq = dx * dx + dy * dy;

        // 如果子弹本帧几乎没动
        if (lineLenSq < 0.0001f)
        {
            float d2 = (cx - p1x) * (cx - p1x) + (cy - p1y) * (cy - p1y);
            return d2 <= r * r;
        }

        // 计算投影比例 t，并限制在 0-1 之间（线段范围内）
        float t = ((cx - p1x) * dx + (cy - p1y) * dy) / lineLenSq;
        t = Mathf.Clamp01(t);

        // 找到线段上距离圆心最近的点
        float closestX = p1x + t * dx;
        float closestY = p1y + t * dy;

        // 检查平方距离，避免开根号运算提升性能
        float distSq = (cx - closestX) * (cx - closestX) + (cy - closestY) * (cy - closestY);
        return distSq <= r * r;
    }

    private void HandleCollision(Entity bullet, Entity enemy, ECSManager ecs, GameConfig config)
    {
        var bulletComp = bullet.GetComponent<BulletComponent>();
        var enemyHealth = enemy.GetComponent<HealthComponent>();
        var enemyPos = enemy.GetComponent<PositionComponent>();
        var bulletPos = bullet.GetComponent<PositionComponent>();

        // 计算碰撞法线方向（从子弹指向敌人），用于击退效果
        float dirX = enemyPos.X - bulletPos.X;
        float dirY = enemyPos.Y - bulletPos.Y;
        float mag = Mathf.Sqrt(dirX * dirX + dirY * dirY);
        if (mag > 0.001f) { dirX /= mag; dirY /= mag; }

        GameObject hitVFXPrefab = null;

        // 根据子弹类型处理不同的业务逻辑
        switch (bulletComp.Type)
        {
            case BulletType.Normal:
                enemyHealth.CurrentHealth -= bulletComp.Damage;
                ApplyKnockback(enemy, dirX, dirY, config);
                hitVFXPrefab = ecs.NormalHitVFX;
                break;

            case BulletType.Slow:
                enemyHealth.CurrentHealth -= bulletComp.Damage;
                ApplyKnockback(enemy, dirX, dirY, config);
                ApplySlowEffect(enemy, config, ecs);
                hitVFXPrefab = ecs.SlowHitVFX;
                break;

            case BulletType.AOE:
                enemyHealth.CurrentHealth -= bulletComp.Damage;
                ApplyKnockback(enemy, dirX, dirY, config);
                ProcessAOE(bulletPos.X, bulletPos.Y, config);
                hitVFXPrefab = ecs.ExplosionVFX;
                break;

            case BulletType.ChainLightning:
                enemyHealth.CurrentHealth -= bulletComp.Damage;
                ApplyKnockback(enemy, dirX, dirY, config);
                ProcessChainLightning(enemy, config, ecs);
                hitVFXPrefab = ecs.LightningHitVFX;
                break;
        }

        // 播放命中特效
        if (hitVFXPrefab != null)
        {
            GameObject vfx = Object.Instantiate(hitVFXPrefab, new Vector3(bulletPos.X, bulletPos.Y, 0), Quaternion.identity);
            Object.Destroy(vfx, 1f);
        }

        // 回收子弹
        ecs.DestroyEntity(bullet);
    }

    private void ApplyKnockback(Entity enemy, float dx, float dy, GameConfig config)
    {
        var enemyComp = enemy.GetComponent<EnemyComponent>();
        enemyComp.KnockbackDirX = dx;
        enemyComp.KnockbackDirY = dy;
        enemyComp.KnockbackSpeed = config.BulletKnockbackSpeed;
        enemyComp.KnockbackTimer = config.BulletKnockbackDuration;
        enemyComp.HitRecoveryTimer = config.BulletHitRecoveryDuration;
    }

    private void ApplySlowEffect(Entity enemy, GameConfig config, ECSManager ecs)
    {
        var slow = enemy.GetComponent<SlowEffectComponent>();
        if (slow == null)
        {
            var pos = enemy.GetComponent<PositionComponent>();
            GameObject vfx = Object.Instantiate(ecs.SlowEffectVFX, new Vector3(pos.X, pos.Y, 0), Quaternion.identity);
            slow = new SlowEffectComponent(config.SlowBulletSlowRatio, config.SlowBulletDuration);
            slow.EffectObject = vfx;
            enemy.AddComponent(slow);
        }
        else
        {
            slow.RemainingDuration = config.SlowBulletDuration;
        }
    }

    private void ProcessAOE(float x, float y, GameConfig config)
    {
        var enemies = GetEntitiesWith<EnemyComponent, PositionComponent, HealthComponent>();
        float rangeSq = config.AOEBulletRadius * config.AOEBulletRadius;
        foreach (var e in enemies)
        {
            if (!e.IsAlive) continue;
            var pos = e.GetComponent<PositionComponent>();
            float dx = pos.X - x;
            float dy = pos.Y - y;
            if (dx * dx + dy * dy <= rangeSq)
            {
                e.GetComponent<HealthComponent>().CurrentHealth -= config.AOEBulletDamage;
            }
        }
    }

    private void ProcessChainLightning(Entity startEnemy, GameConfig config, ECSManager ecs)
    {
        List<Entity> hitHistory = new List<Entity> { startEnemy };
        Entity current = startEnemy;
        
        var allEnemies = GetEntitiesWith<EnemyComponent, PositionComponent, HealthComponent>();

        for (int i = 0; i < config.ChainLightningMaxTargets - 1; i++)
        {
            Entity next = null;
            float bestDistSq = config.ChainLightningChainRange * config.ChainLightningChainRange;
            var curPos = current.GetComponent<PositionComponent>();

            // 寻找最近的下一个目标
            foreach (var potential in allEnemies)
            {
                if (!potential.IsAlive || hitHistory.Contains(potential)) continue;

                var potPos = potential.GetComponent<PositionComponent>();
                float dx = potPos.X - curPos.X;
                float dy = potPos.Y - curPos.Y;
                float d2 = dx * dx + dy * dy;

                if (d2 < bestDistSq)
                {
                    bestDistSq = d2;
                    next = potential;
                }
            }

            if (next != null)
            {
                // 应用连环伤害
                next.GetComponent<HealthComponent>().CurrentHealth -= config.ChainLightningDamage;
                hitHistory.Add(next);

                // 创建闪电连线 VFX
                CreateChainVFX(current, next, ecs);

                current = next;
            }
            else break; // 范围内没有可用目标，跳出连锁
        }
    }

    private void CreateChainVFX(Entity from, Entity to, ECSManager ecs)
    {
        var start = from.GetComponent<PositionComponent>();
        var end = to.GetComponent<PositionComponent>();

        // 创建一个新的 VFX 实体
        Entity vfxEntity = ecs.CreateEntity();
    
        // 1. 添加数据组件 (起止点, 持续0.15秒, 抖动0.3f, 6个分段)
        vfxEntity.AddComponent(new LightningVFXComponent(
            new Vector3(start.X, start.Y, 0), 
            new Vector3(end.X, end.Y, 0), 
            0.15f, 0.3f, 6));

        // 2. 添加视图组件（从池里拿一个空的特效 GameObject）
        // 注意：这里的 LightningChainVFX 预制体应该挂有 LineRenderer
        GameObject go = Object.Instantiate(ecs.LightningChainVFX); 
        vfxEntity.AddComponent(new ViewComponent(go));
    }
}