using System.Collections.Generic;
using UnityEngine;

public class PlayerShootingSystem : SystemBase
{
    private float _shootTimer;
    private GridSystem _grid;
    public static BulletType CurrentBulletType = BulletType.Normal; // 可以在外部修改此类型测试
    
    public PlayerShootingSystem(List<Entity> entities, GridSystem grid) : base(entities) { _grid = grid; }
    
    public override void Update(float deltaTime)
    {
        var config = ECSManager.Instance.Config;
        float interval = GetInterval(config);
        
        _shootTimer += deltaTime;
        if (_shootTimer >= interval)
        {
            _shootTimer = 0;
            Shoot(config);
        }
    }
    
    void Shoot(GameConfig config)
    {
        var ecs = ECSManager.Instance;
        var player = ecs.PlayerEntity;
        if (player == null || !player.IsAlive) return;
        
        var pPos = player.GetComponent<PositionComponent>();
        Entity target = FindNearestInGrid(pPos.X, pPos.Y);
        if (target == null) return;
        
        var tPos = target.GetComponent<PositionComponent>();
        Vector2 dir = new Vector2(tPos.X - pPos.X, tPos.Y - pPos.Y).normalized;
        
        // 1. 获取对应的预制体
        GameObject prefab = PoolManager.Instance.GetBulletPrefab(CurrentBulletType);
        if (prefab == null) 
        {
            Debug.LogError($"PoolManager 中未分配 {CurrentBulletType} 的预制体！");
            return;
        }

        GameObject bulletGo = PoolManager.Instance.Spawn(prefab, new Vector3(pPos.X, pPos.Y, 0), Quaternion.identity);
        
        // 2. 创建 ECS 实体并按原子化方案组装组件
        Entity bullet = ecs.CreateEntity();
        bullet.AddComponent(new BulletTag()); 
        bullet.AddComponent(new PositionComponent(pPos.X, pPos.Y, 0));
        bullet.AddComponent(new VelocityComponent(dir.x * config.BulletSpeed, dir.y * config.BulletSpeed, 0));
        bullet.AddComponent(new ViewComponent(bulletGo));
        bullet.AddComponent(new LifetimeComponent { RemainingTime = config.BulletLifeTime });
        bullet.AddComponent(new CollisionComponent(config.BulletCollisionRadius));
        bullet.AddComponent(new TraceComponent(pPos.X, pPos.Y));

        // --- 核心修复：必须添加具体的效果组件，否则 BulletEffectSystem 无法处理 ---
        switch (CurrentBulletType)
        {
            case BulletType.Normal:
                bullet.AddComponent(new DamageComponent(config.BulletDamage));
                break;
            case BulletType.Slow:
                bullet.AddComponent(new DamageComponent(config.BulletDamage * 0.5f));
                bullet.AddComponent(new SlowEffectComponent(config.SlowRatio, config.SlowDuration));
                break;
            case BulletType.ChainLightning:
                // 只有挂载了 ChainComponent，击中后才会有闪电效果
                bullet.AddComponent(new ChainComponent(config.ChainTargets, config.ChainRange, config.ChainDamage));
                break;
            case BulletType.AOE:
                bullet.AddComponent(new AOEComponent(config.AOERadius, config.AOEDamage));
                break;
        }
    }

    private float GetInterval(GameConfig config) => CurrentBulletType switch {
        BulletType.Slow => config.SlowBulletShootInterval,
        BulletType.ChainLightning => config.ChainLightningShootInterval,
        BulletType.AOE => config.AOEBulletShootInterval,
        _ => config.ShootInterval
    };
    
    private Entity FindNearestInGrid(float x, float y)
    {
        var nearbyEnemies = _grid.GetNearbyEnemies(x, y);
        Entity nearest = null;
        float minDistSq = float.MaxValue;
        foreach (var e in nearbyEnemies)
        {
            if (!e.IsAlive) continue;
            var ePos = e.GetComponent<PositionComponent>();
            float d2 = (ePos.X - x) * (ePos.X - x) + (ePos.Y - y) * (ePos.Y - y);
            if (d2 < minDistSq) { minDistSq = d2; nearest = e; }
        }
        return nearest;
    }
}