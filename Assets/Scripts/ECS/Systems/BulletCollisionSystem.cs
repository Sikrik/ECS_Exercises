using System.Collections.Generic;
using UnityEngine;

public class BulletCollisionSystem : SystemBase
{
    public BulletCollisionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        CheckBulletEnemyCollision();
    }

    /// <summary>
    /// 检测子弹与敌人的碰撞
    /// </summary>
    void CheckBulletEnemyCollision()
    {
        // 仅筛选存活的子弹和敌人
        var bullets = GetEntitiesWith<BulletComponent, PositionComponent, CollisionComponent, VelocityComponent>();
        var enemies = GetEntitiesWith<EnemyComponent, PositionComponent, CollisionComponent, HealthComponent>();
        
        var ecs = ECSManager.Instance;
        var config = ecs.Config;
        
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            var bullet = bullets[i];
            if (bullet == null || !bullet.IsAlive) continue; // 存活检查
            
            var bulletPos = bullet.GetComponent<PositionComponent>();
            var bulletCol = bullet.GetComponent<CollisionComponent>();
            var bulletComp = bullet.GetComponent<BulletComponent>();
            
            for (int j = enemies.Count - 1; j >= 0; j--)
            {
                var enemy = enemies[j];
                if (enemy == null || !enemy.IsAlive) continue; // 存活检查
                
                var enemyPos = enemy.GetComponent<PositionComponent>();
                var enemyCol = enemy.GetComponent<CollisionComponent>();
                var enemyComp = enemy.GetComponent<EnemyComponent>();
                var enemyHealth = enemy.GetComponent<HealthComponent>();
                
                // 碰撞半径之和
                float r = bulletCol.Radius + enemyCol.Radius;
                
                // 使用“线段最近点”算法检测碰撞，彻底解决穿透问题
                bool isCollided = CheckSegmentCircleIntersection(
                    bulletPos.PreviousX, bulletPos.PreviousY, 
                    bulletPos.X, bulletPos.Y, 
                    enemyPos.X, enemyPos.Y, r);
                
                if (isCollided)
                {
                    // 计算碰撞方向，用于击退
                    float dirX = enemyPos.X - bulletPos.X;
                    float dirY = enemyPos.Y - bulletPos.Y;
                    float dirMag = Mathf.Sqrt(dirX * dirX + dirY * dirY);
                    if (dirMag > 0.1f) { dirX /= dirMag; dirY /= dirMag; }
                    
                    GameObject hitVFX = null;
                    // 处理不同子弹类型的效果
                    switch (bulletComp.Type)
                    {
                        case BulletType.Normal:
                            enemyHealth.CurrentHealth -= bulletComp.Damage;
                            InitEnemyKnockback(enemy, enemyComp, dirX, dirY, config);
                            hitVFX = ecs.NormalHitVFX;
                            break;
                            
                        case BulletType.Slow:
                            enemyHealth.CurrentHealth -= bulletComp.Damage;
                            InitEnemyKnockback(enemy, enemyComp, dirX, dirY, config);
                            var slowEffect = enemy.GetComponent<SlowEffectComponent>();
                            if (slowEffect == null)
                            {
                                GameObject slowVFX = Object.Instantiate(ecs.SlowEffectVFX, new Vector3(enemyPos.X, enemyPos.Y, 0), Quaternion.identity);
                                slowEffect = new SlowEffectComponent(config.SlowBulletSlowRatio, config.SlowBulletDuration);
                                slowEffect.EffectObject = slowVFX;
                                enemy.AddComponent(slowEffect);
                            }
                            else
                            {
                                slowEffect.RemainingDuration = config.SlowBulletDuration;
                            }
                            hitVFX = ecs.SlowHitVFX;
                            break;
                            
                        case BulletType.AOE:
                            enemyHealth.CurrentHealth -= bulletComp.Damage;
                            InitEnemyKnockback(enemy, enemyComp, dirX, dirY, config);
                            ProcessAOEEffect(bulletPos, enemies, config);
                            hitVFX = ecs.ExplosionVFX;
                            break;
                            
                        case BulletType.ChainLightning:
                            enemyHealth.CurrentHealth -= config.ChainLightningDamage;
                            InitEnemyKnockback(enemy, enemyComp, dirX, dirY, config);
                            ProcessChainLightningEffect(enemy, bulletPos, enemies, config, ecs);
                            hitVFX = ecs.LightningHitVFX;
                            break;
                    }
                    
                    if (hitVFX != null)
                    {
                        GameObject vfx = Object.Instantiate(hitVFX, new Vector3(bulletPos.X, bulletPos.Y, 0), Quaternion.identity);
                        Object.Destroy(vfx, 2f);
                    }
                    
                    // 销毁子弹并立即跳出
                    ecs.DestroyEntity(bullet);
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// 线段与圆碰撞检测（最近点法）：通过检测子弹运动轨迹线段上离圆心最近的点是否在圆内来判断
    /// </summary>
    private bool CheckSegmentCircleIntersection(float p1x, float p1y, float p2x, float p2y, float cx, float cy, float r)
    {
        float dx = p2x - p1x;
        float dy = p2y - p1y;
        float d2 = dx * dx + dy * dy;

        if (d2 < 0.0001f) // 子弹未移动
        {
            return (cx - p1x) * (cx - p1x) + (cy - p1y) * (cy - p1y) <= r * r;
        }

        // 计算投影点在线段上的比例 t
        float t = Mathf.Clamp01(((cx - p1x) * dx + (cy - p1y) * dy) / d2);

        // 线段上距离圆心最近的点
        float closestX = p1x + t * dx;
        float closestY = p1y + t * dy;

        float distSq = (cx - closestX) * (cx - closestX) + (cy - closestY) * (cy - closestY);
        return distSq <= r * r;
    }
    
    void InitEnemyKnockback(Entity enemy, EnemyComponent enemyComp, float dirX, float dirY, GameConfig config)
    {
        if (config == null) return;
        float knockbackSpeed = config.BulletKnockbackSpeed;
        if (enemyComp.Type == EnemyType.Fast) knockbackSpeed *= 1.2f;
        else if (enemyComp.Type == EnemyType.Tank) knockbackSpeed *= 0.5f;

        enemyComp.KnockbackDirX = dirX;
        enemyComp.KnockbackDirY = dirY;
        enemyComp.KnockbackSpeed = knockbackSpeed;
        enemyComp.KnockbackTimer = config.BulletKnockbackDuration;
        enemyComp.HitRecoveryTimer = config.BulletHitRecoveryDuration;
    }
    
    void ProcessAOEEffect(PositionComponent hitPos, List<Entity> enemies, GameConfig config)
    {
        float radiusSq = config.AOEBulletRadius * config.AOEBulletRadius;
        foreach (var enemy in enemies)
        {
            if (enemy == null || !enemy.IsAlive) continue;
            var pos = enemy.GetComponent<PositionComponent>();
            var health = enemy.GetComponent<HealthComponent>();
            float dx = pos.X - hitPos.X;
            float dy = pos.Y - hitPos.Y;
            if (dx * dx + dy * dy < radiusSq) health.CurrentHealth -= config.AOEBulletDamage;
        }
    }

    void ProcessChainLightningEffect(Entity firstTarget, PositionComponent hitPos, List<Entity> enemies, GameConfig config, ECSManager ecs)
    {
        List<Entity> hitTargets = new List<Entity> { firstTarget };
        Entity currentTarget = firstTarget;
        PositionComponent currentPos = hitPos;

        for (int i = 0; i < config.ChainLightningMaxTargets - 1; i++)
        {
            Entity nextTarget = null;
            float minDistSq = config.ChainLightningChainRange * config.ChainLightningChainRange;
            PositionComponent nextPos = null;

            foreach (var enemy in enemies)
            {
                if (enemy == null || !enemy.IsAlive || hitTargets.Contains(enemy)) continue;
                var pos = enemy.GetComponent<PositionComponent>();
                float dx = pos.X - currentPos.X;
                float dy = pos.Y - currentPos.Y;
                float distSq = dx * dx + dy * dy;

                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    nextTarget = enemy;
                    nextPos = pos;
                }
            }

            if (nextTarget != null)
            {
                nextTarget.GetComponent<HealthComponent>().CurrentHealth -= config.ChainLightningDamage;
                if (ecs.LightningChainVFX != null)
                {
                    // 特效生成逻辑
                    Vector3 start = new Vector3(currentPos.X, currentPos.Y, 0);
                    Vector3 end = new Vector3(nextPos.X, nextPos.Y, 0);
                    GameObject chain = Object.Instantiate(ecs.LightningChainVFX, (start + end) / 2, Quaternion.Euler(0, 0, Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg));
                    chain.transform.localScale = new Vector3(Vector3.Distance(start, end), 1, 1);
                    Object.Destroy(chain, 0.5f);
                }
                hitTargets.Add(nextTarget);
                currentTarget = nextTarget;
                currentPos = nextPos;
            }
            else break;
        }
    }
}