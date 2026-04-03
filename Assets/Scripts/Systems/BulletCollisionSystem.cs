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
        var bullets = GetEntitiesWith<BulletComponent, PositionComponent, CollisionComponent, VelocityComponent>();
        var enemies = GetEntitiesWith<EnemyComponent, PositionComponent, CollisionComponent, HealthComponent>();
        
        // 先获取配置与ECS实例
        var ecs = ECSManager.Instance;
        var config = ecs.Config;
        
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            var bullet = bullets[i];
            if (bullet == null) continue;
            
            var bulletPos = bullet.GetComponent<PositionComponent>();
            var bulletCol = bullet.GetComponent<CollisionComponent>();
            var bulletComp = bullet.GetComponent<BulletComponent>();
            var bulletVel = bullet.GetComponent<VelocityComponent>();
            if (bulletPos == null || bulletCol == null || bulletComp == null || bulletVel == null) continue;
            
            for (int j = enemies.Count - 1; j >= 0; j--)
            {
                var enemy = enemies[j];
                if (enemy == null) continue;
                
                var enemyPos = enemy.GetComponent<PositionComponent>();
                var enemyCol = enemy.GetComponent<CollisionComponent>();
                var enemyComp = enemy.GetComponent<EnemyComponent>();
                var enemyHealth = enemy.GetComponent<HealthComponent>();
                if (enemyPos == null || enemyCol == null || enemyComp == null || enemyHealth == null) continue;
                
                // 使用线段与圆的相交检测，解决高速子弹穿透敌人的问题
                float p1x = bulletPos.PreviousX;
                float p1y = bulletPos.PreviousY;
                float p2x = bulletPos.X;
                float p2y = bulletPos.Y;
                float cx = enemyPos.X;
                float cy = enemyPos.Y;
                float r = bulletCol.Radius + enemyCol.Radius;
                
                bool isCollided = LineCircleIntersect(p1x, p1y, p2x, p2y, cx, cy, r);
                
                if (isCollided)
                {
                    // 计算碰撞方向，用于击退敌人
                    float dirX = enemyPos.X - bulletPos.X;
                    float dirY = enemyPos.Y - bulletPos.Y;
                    float dirMag = Mathf.Sqrt(dirX * dirX + dirY * dirY);
                    if (dirMag > 0.1f)
                    {
                        dirX /= dirMag;
                        dirY /= dirMag;
                    }
                    
                    GameObject hitVFX = null;
                    // 根据子弹类型处理不同的特殊效果
                    switch (bulletComp.Type)
                    {
                        case BulletType.Normal:
                            // 普通子弹：基础伤害+击退
                            enemyHealth.CurrentHealth -= bulletComp.Damage;
                            InitEnemyKnockback(enemy, enemyComp, dirX, dirY, config);
                            hitVFX = ecs.NormalHitVFX;
                            break;
                            
                        case BulletType.Slow:
                            // 减速子弹：伤害+击退+减速效果
                            enemyHealth.CurrentHealth -= bulletComp.Damage;
                            InitEnemyKnockback(enemy, enemyComp, dirX, dirY, config);
                            // 添加/刷新减速效果
                            var slowEffect = enemy.GetComponent<SlowEffectComponent>();
                            if (slowEffect == null)
                            {
                                // 实例化减速持续特效
                                GameObject slowVFX = Object.Instantiate(ecs.SlowEffectVFX, new Vector3(enemyPos.X, enemyPos.Y, 0), Quaternion.identity);
                                slowEffect = new SlowEffectComponent(config.SlowBulletSlowRatio, config.SlowBulletDuration);
                                slowEffect.EffectObject = slowVFX;
                                enemy.AddComponent(slowEffect);
                            }
                            else
                            {
                                // 刷新已有的减速效果，重置持续时间
                                slowEffect.RemainingDuration = config.SlowBulletDuration;
                                slowEffect.SlowRatio = config.SlowBulletSlowRatio;
                            }
                            hitVFX = ecs.SlowHitVFX;
                            break;
                            
                        case BulletType.AOE:
                            // AOE范围子弹：仅直接击中的敌人被击退，范围敌人只受伤害
                            enemyHealth.CurrentHealth -= bulletComp.Damage;
                            InitEnemyKnockback(enemy, enemyComp, dirX, dirY, config);
                            // 范围伤害，不击退
                            ProcessAOEEffect(bulletPos, enemies, config);
                            hitVFX = ecs.ExplosionVFX;
                            break;
                            
                        case BulletType.ChainLightning:
                            // 连锁闪电子弹：仅第一个目标被击退，后续连锁目标只受伤害
                            enemyHealth.CurrentHealth -= config.ChainLightningDamage;
                            InitEnemyKnockback(enemy, enemyComp, dirX, dirY, config);
                            // 连锁伤害，不击退，同时生成连锁特效
                            ProcessChainLightningEffect(enemy, bulletPos, enemies, config, ecs);
                            hitVFX = ecs.LightningHitVFX;
                            break;
                    }
                    
                    // 生成命中特效
                    if (hitVFX != null)
                    {
                        GameObject vfx = Object.Instantiate(hitVFX, new Vector3(bulletPos.X, bulletPos.Y, 0), Quaternion.identity);
                        Object.Destroy(vfx, 2f);
                    }
                    
                    // 击中敌人后销毁子弹，自动回收到对象池
                    ECSManager.Instance.DestroyEntity(bullet);
                    
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// 线段与圆的相交检测算法，用于解决高速子弹的穿透问题
    /// 检测子弹的移动线段是否与敌人的圆形碰撞体相交
    /// </summary>
    /// <param name="p1x">线段起点X（子弹上一帧位置）</param>
    /// <param name="p1y">线段起点Y（子弹上一帧位置）</param>
    /// <param name="p2x">线段终点X（子弹当前帧位置）</param>
    /// <param name="p2y">线段终点Y（子弹当前帧位置）</param>
    /// <param name="cx">圆心X（敌人位置）</param>
    /// <param name="cy">圆心Y（敌人位置）</param>
    /// <param name="r">圆半径（子弹半径+敌人半径）</param>
    /// <returns>是否相交</returns>
    private bool LineCircleIntersect(float p1x, float p1y, float p2x, float p2y, float cx, float cy, float r)
    {
        // 线段向量
        float dx = p2x - p1x;
        float dy = p2y - p1y;
        // 圆心到线段起点的向量
        float fx = p1x - cx;
        float fy = p1y - cy;
        
        // 二次方程参数：a*t² + b*t + c = 0
        float a = dx * dx + dy * dy; // 线段长度的平方
        float b = 2 * (fx * dx + fy * dy);
        float c = (fx * fx + fy * fy) - r * r;
        
        // 计算判别式，判断是否有实根
        float discriminant = b * b - 4 * a * c;
        if (discriminant < 0)
        {
            // 没有实根，线段与圆完全不相交
            return false;
        }
        
        // 计算根
        discriminant = Mathf.Sqrt(discriminant);
        float t1 = (-b - discriminant) / (2 * a);
        float t2 = (-b + discriminant) / (2 * a);
        
        // 只要有一个根在[0,1]范围内，说明线段与圆相交
        // t=0对应线段起点，t=1对应线段终点
        if ((t1 >= 0 && t1 <= 1) || (t2 >= 0 && t2 <= 1))
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 初始化敌人的击退状态
    /// </summary>
    void InitEnemyKnockback(Entity enemy, EnemyComponent enemyComp, float dirX, float dirY, GameConfig config)
    {
        if (config == null) return;
        
        float knockbackSpeed, knockbackDuration, hitRecoveryDuration;
        switch (enemyComp.Type)
        {
            case EnemyType.Fast:
                knockbackSpeed = config.BulletKnockbackSpeed * 1.2f;
                knockbackDuration = config.BulletKnockbackDuration;
                hitRecoveryDuration = config.BulletHitRecoveryDuration;
                break;
            case EnemyType.Tank:
                knockbackSpeed = config.BulletKnockbackSpeed * 0.5f;
                knockbackDuration = config.BulletKnockbackDuration;
                hitRecoveryDuration = config.BulletHitRecoveryDuration;
                break;
            case EnemyType.Normal:
            default:
                knockbackSpeed = config.BulletKnockbackSpeed;
                knockbackDuration = config.BulletKnockbackDuration;
                hitRecoveryDuration = config.BulletHitRecoveryDuration;
                break;
        }
        
        enemyComp.KnockbackDirX = dirX;
        enemyComp.KnockbackDirY = dirY;
        enemyComp.KnockbackSpeed = knockbackSpeed;
        enemyComp.KnockbackTimer = knockbackDuration;
        enemyComp.HitRecoveryTimer = hitRecoveryDuration;
    }
    
    /// <summary>
    /// 处理AOE范围伤害效果，仅伤害，不击退
    /// </summary>
    void ProcessAOEEffect(PositionComponent hitPos, List<Entity> enemies, GameConfig config)
    {
        float radiusSq = config.AOEBulletRadius * config.AOEBulletRadius;
        
        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            
            var enemyPos = enemy.GetComponent<PositionComponent>();
            var enemyHealth = enemy.GetComponent<HealthComponent>();
            if (enemyPos == null || enemyHealth == null) continue;
            
            // 计算敌人与爆炸中心的距离
            float dx = enemyPos.X - hitPos.X;
            float dy = enemyPos.Y - hitPos.Y;
            float distSq = dx * dx + dy * dy;
            
            if (distSq < radiusSq)
            {
                // 范围伤害，不击退
                enemyHealth.CurrentHealth -= config.AOEBulletDamage;
            }
        }
    }
    
    /// <summary>
    /// 处理连锁闪电效果，仅第一个目标被击退，后续只伤害，同时生成连锁特效
    /// </summary>
    void ProcessChainLightningEffect(Entity firstTarget, PositionComponent hitPos, List<Entity> enemies, GameConfig config, ECSManager ecs)
    {
        List<Entity> hitTargets = new List<Entity>();
        Entity currentTarget = firstTarget;
        PositionComponent currentPos = hitPos;
        
        for (int i = 0; i < config.ChainLightningMaxTargets - 1; i++) // 第一个已经处理过了
        {
            if (currentTarget == null) break;
            if (hitTargets.Contains(currentTarget)) break;
            
            hitTargets.Add(currentTarget);
            
            // 找下一个最近的未命中的敌人
            Entity nextTarget = null;
            float minDistSq = config.ChainLightningChainRange * config.ChainLightningChainRange;
            PositionComponent nextPos = null;
            
            foreach (var enemy in enemies)
            {
                if (enemy == null) continue;
                if (hitTargets.Contains(enemy)) continue;
                if (enemy == firstTarget) continue; // 第一个已经处理过了
                
                var pos = enemy.GetComponent<PositionComponent>();
                if (pos == null) continue;
                
                float dx = pos.X - currentPos.X;
                float dy = pos.Y - currentPos.Y;
                float distSq = dx*dx + dy*dy;
                
                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    nextTarget = enemy;
                    nextPos = pos;
                }
            }
            
            if (nextTarget != null && nextPos != null)
            {
                // 对下一个目标造成伤害，不击退
                var enemyHealth = nextTarget.GetComponent<HealthComponent>();
                if (enemyHealth != null)
                {
                    enemyHealth.CurrentHealth -= config.ChainLightningDamage;
                }
                
                // 生成连锁闪电特效
                if (ecs.LightningChainVFX != null)
                {
                    Vector3 startPos = new Vector3(currentPos.X, currentPos.Y, 0);
                    Vector3 endPos = new Vector3(nextPos.X, nextPos.Y, 0);
                    Vector3 center = (startPos + endPos) / 2;
                    float distance = Vector3.Distance(startPos, endPos);
                    float angle = Mathf.Atan2(endPos.y - startPos.y, endPos.x - startPos.x) * Mathf.Rad2Deg;
                    
                    GameObject chainVFX = Object.Instantiate(ecs.LightningChainVFX, center, Quaternion.Euler(0, 0, angle));
                    chainVFX.transform.localScale = new Vector3(distance, 1, 1);
                    Object.Destroy(chainVFX, 0.5f);
                }
                
                // 继续下一次连锁
                currentTarget = nextTarget;
                currentPos = nextPos;
            }
            else
            {
                break;
            }
        }
    }
}