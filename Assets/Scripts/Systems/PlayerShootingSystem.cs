// PlayerShootingSystem.cs 修复后版本
// 修复内容：
// 1. 移除废弃的bulletPrefab构造参数，改为从ECSManager获取对应子弹池
// 2. 修复创建子弹时的逻辑，改为从对应子弹对象池获取对象，而非直接Instantiate，启用对象池优化
// 3. 移除无用的旧预制体参数，适配新的多子弹池设计
// 4. 新增自动同步子弹碰撞半径与Sprite视觉大小的逻辑
using System.Collections.Generic;
using UnityEngine;
public class PlayerShootingSystem : SystemBase
{
    private float _shootTimer;
    private float _currentShootInterval;
    
    // 静态变量，存储当前选中的子弹类型
    public static BulletType CurrentBulletType = BulletType.Normal;
    
    public PlayerShootingSystem(List<Entity> entities) : base(entities) 
    {
        _currentShootInterval = ECSManager.Instance.Config.ShootInterval;
    }
    
    public override void Update(float deltaTime)
    {
        // 根据当前子弹类型更新射击间隔
        var config = ECSManager.Instance.Config;
        switch(CurrentBulletType)
        {
            case BulletType.Normal:
                _currentShootInterval = config.ShootInterval;
                break;
            case BulletType.Slow:
                _currentShootInterval = config.SlowBulletShootInterval;
                break;
            case BulletType.ChainLightning:
                _currentShootInterval = config.ChainLightningShootInterval;
                break;
            case BulletType.AOE:
                _currentShootInterval = config.AOEBulletShootInterval;
                break;
        }
        
        _shootTimer += deltaTime;
        if (_shootTimer >= _currentShootInterval)
        {
            _shootTimer = 0;
            Shoot();
        }
    }
    
    void Shoot()
    {
        var config = ECSManager.Instance.Config;
        var player = ECSManager.Instance.PlayerEntity;
        if (player == null) return;
        
        var playerPos = player.GetComponent<PositionComponent>();
        Entity nearestEnemy = FindNearestEnemy(playerPos);
        
        if (nearestEnemy == null) return;
        var enemyPos = nearestEnemy.GetComponent<PositionComponent>();
        
        // 计算射击方向
        float dirX = enemyPos.X - playerPos.X;
        float dirY = enemyPos.Y - playerPos.Y;
        Vector2 dir = new Vector2(dirX, dirY).normalized;
        
        // 创建子弹实体
        Entity bullet = ECSManager.Instance.CreateEntity();
        float bulletSpeed = config.BulletSpeed; 
        
        bullet.AddComponent(new PositionComponent(playerPos.X, playerPos.Y, 0));
        bullet.AddComponent(new VelocityComponent(dir.x * bulletSpeed, dir.y * bulletSpeed, 0));
        
        // 根据子弹类型配置属性
        BulletComponent bulletComp = new BulletComponent();
        bulletComp.Type = CurrentBulletType;
        
        ObjectPool bulletPool = null;
        switch(CurrentBulletType)
        {
            case BulletType.Normal:
                bulletComp.Damage = config.BulletDamage;
                bulletComp.LifeTime = config.BulletLifeTime;
                bulletPool = ECSManager.Instance.NormalBulletPool;
                break;
            case BulletType.Slow:
                bulletComp.Damage = config.SlowBulletDamage;
                bulletComp.LifeTime = config.BulletLifeTime;
                bulletPool = ECSManager.Instance.SlowBulletPool;
                break;
            case BulletType.ChainLightning:
                bulletComp.Damage = config.ChainLightningDamage;
                bulletComp.LifeTime = config.BulletLifeTime;
                bulletPool = ECSManager.Instance.ChainLightningBulletPool;
                break;
            case BulletType.AOE:
                bulletComp.Damage = config.AOEBulletDamage;
                bulletComp.LifeTime = config.BulletLifeTime;
                bulletPool = ECSManager.Instance.AOEBulletPool;
                break;
        }
        
        bullet.AddComponent(bulletComp);
        
        // 先从对象池获取子弹GameObject，用于自动同步碰撞半径
        GameObject bulletGo = bulletPool.Get();
        float bulletCollisionRadius = config.BulletCollisionRadius; // 默认用配置兜底
        // 自动同步子弹的碰撞半径与Sprite视觉大小
        if (bulletGo.TryGetComponent<SpriteRenderer>(out var bulletSprite))
        {
            float spriteWidth = bulletSprite.bounds.size.x;
            float spriteHeight = bulletSprite.bounds.size.y;
            bulletCollisionRadius = Mathf.Min(spriteWidth, spriteHeight) * 0.5f;
        }
        
        bulletGo.transform.position = new Vector3(playerPos.X, playerPos.Y, 0);
        bulletGo.transform.rotation = Quaternion.identity;
        
        // 使用自动同步后的半径创建碰撞组件
        bullet.AddComponent(new CollisionComponent(bulletCollisionRadius));
        
        bullet.AddComponent(new ViewComponent(bulletGo));
    }
    
    // 查找最近敌人（保留原有逻辑）
    Entity FindNearestEnemy(PositionComponent playerPos)
    {
        var enemies = GetEntitiesWith<EnemyComponent, PositionComponent>();
        if (enemies.Count == 0) return null;
        
        Entity nearest = null;
        float minDist = float.MaxValue;
        
        foreach (var enemy in enemies)
        {
            var enemyPos = enemy.GetComponent<PositionComponent>();
            float dist = Vector2.Distance(
                new Vector2(playerPos.X, playerPos.Y),
                new Vector2(enemyPos.X, enemyPos.Y)
            );
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemy;
            }
        }
        return nearest;
    }
}