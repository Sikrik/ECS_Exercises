using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家射击系统：负责子弹的生产、属性初始化及自动目标锁定
/// </summary>
public class PlayerShootingSystem : SystemBase
{
    private float _shootTimer;
    private float _currentShootInterval;
    
    // 静态变量，存储当前选中的子弹类型（UI切换时修改此值）
    public static BulletType CurrentBulletType = BulletType.Normal;
    
    public PlayerShootingSystem(List<Entity> entities) : base(entities) 
    {
        if (ECSManager.Instance.Config != null)
            _currentShootInterval = ECSManager.Instance.Config.ShootInterval;
    }
    
    public override void Update(float deltaTime)
    {
        var config = ECSManager.Instance.Config;
        if (config == null) return;

        // 根据当前选中的子弹类型动态调整射击频率
        _currentShootInterval = CurrentBulletType switch
        {
            BulletType.Slow => config.SlowBulletShootInterval,
            BulletType.ChainLightning => config.ChainLightningShootInterval,
            BulletType.AOE => config.AOEBulletShootInterval,
            _ => config.ShootInterval
        };
        
        _shootTimer += deltaTime;
        if (_shootTimer >= _currentShootInterval)
        {
            _shootTimer = 0;
            Shoot();
        }
    }
    
    void Shoot()
    {
        var ecs = ECSManager.Instance;
        var player = ecs.PlayerEntity;
        if (player == null || !player.IsAlive) return;
        
        var playerPos = player.GetComponent<PositionComponent>();
        
        // 1. 寻找最近的敌人作为目标
        Entity target = FindNearestEnemy(playerPos);
        if (target == null) return;
        
        var targetPos = target.GetComponent<PositionComponent>();
        Vector2 dir = new Vector2(targetPos.X - playerPos.X, targetPos.Y - playerPos.Y).normalized;
        
        // 2. 核心架构优化：从 PoolManager 获取对应的子弹预制体并生成
        GameObject prefab = PoolManager.Instance.GetBulletPrefab(CurrentBulletType);
        GameObject bulletGo = PoolManager.Instance.Spawn(prefab, new Vector3(playerPos.X, playerPos.Y, 0), Quaternion.identity);
        
        // 3. 创建子弹实体并挂载基础组件
        Entity bullet = ecs.CreateEntity();
        float speed = ecs.Config.BulletSpeed;
        
        bullet.AddComponent(new PositionComponent(playerPos.X, playerPos.Y, 0));
        bullet.AddComponent(new VelocityComponent(dir.x * speed, dir.y * speed, 0));
        bullet.AddComponent(new ViewComponent(bulletGo));
        
        // 4. 根据类型配置业务数据组件
        BulletComponent bulletComp = new BulletComponent { Type = CurrentBulletType };
        var config = ecs.Config;
        
        switch(CurrentBulletType)
        {
            case BulletType.Slow:
                bulletComp.Damage = config.SlowBulletDamage;
                bulletComp.LifeTime = config.BulletLifeTime;
                break;
            case BulletType.ChainLightning:
                bulletComp.Damage = config.ChainLightningDamage;
                bulletComp.LifeTime = config.BulletLifeTime;
                break;
            case BulletType.AOE:
                bulletComp.Damage = config.AOEBulletDamage;
                bulletComp.LifeTime = config.BulletLifeTime;
                break;
            default:
                bulletComp.Damage = config.BulletDamage;
                bulletComp.LifeTime = config.BulletLifeTime;
                break;
        }
        bullet.AddComponent(bulletComp);

        // 5. 自动同步碰撞半径与视觉大小
        float radius = config.BulletCollisionRadius;
        if (bulletGo.TryGetComponent<SpriteRenderer>(out var sr))
        {
            radius = Mathf.Min(sr.bounds.size.x, sr.bounds.size.y) * 0.5f;
        }
        bullet.AddComponent(new CollisionComponent(radius));
    }
    
    Entity FindNearestEnemy(PositionComponent playerPos)
    {
        var enemies = GetEntitiesWith<EnemyComponent, PositionComponent>();
        Entity nearest = null;
        float minDist = float.MaxValue;
        
        foreach (var enemy in enemies)
        {
            if (!enemy.IsAlive) continue;
            var enemyPos = enemy.GetComponent<PositionComponent>();
            float dist = Vector2.SqrMagnitude(new Vector2(playerPos.X - enemyPos.X, playerPos.Y - enemyPos.Y));
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemy;
            }
        }
        return nearest;
    }
}