using System.Collections.Generic;
using UnityEngine;

public class PlayerShootingSystem : SystemBase
{
    private float _shootTimer;
    private GridSystem _grid;
    public static BulletType CurrentBulletType = BulletType.Normal;
    
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
        var player = ecs.PlayerEntity; // 假设 ecs.PlayerEntity 已经重构为带 PlayerTag
        if (player == null || !player.IsAlive) return;
        
        var pPos = player.GetComponent<PositionComponent>();
        Entity target = FindNearestInGrid(pPos.X, pPos.Y);
        if (target == null) return;
        
        var tPos = target.GetComponent<PositionComponent>();
        Vector2 dir = new Vector2(tPos.X - pPos.X, tPos.Y - pPos.Y).normalized;
        
        // 生成视觉对象
        GameObject prefab = PoolManager.Instance.GetBulletPrefab(CurrentBulletType);
        GameObject bulletGo = PoolManager.Instance.Spawn(prefab, new Vector3(pPos.X, pPos.Y, 0), Quaternion.identity);
        
        // --- 核心重构：组合原子组件 ---
        Entity bullet = ecs.CreateEntity();
        bullet.AddComponent(new BulletTag()); // 身份标记
        bullet.AddComponent(new PositionComponent(pPos.X, pPos.Y, 0));
        bullet.AddComponent(new VelocityComponent(dir.x * config.BulletSpeed, dir.y * config.BulletSpeed, 0));
        bullet.AddComponent(new ViewComponent(bulletGo));
        
        // 战斗属性
        bullet.AddComponent(new BulletStatsComponent { 
            Type = CurrentBulletType, 
            Damage = config.BulletDamage 
        });

        // 生命周期（通用组件代替旧的 BulletComponent.LifeTime）
        bullet.AddComponent(new LifetimeComponent { 
            RemainingTime = config.BulletLifeTime 
        });

        // 轨迹追踪（用于防穿透，由 MovementSystem 使用）
        bullet.AddComponent(new TraceComponent(pPos.X, pPos.Y));
        
        // 碰撞数据
        bullet.AddComponent(new CollisionComponent(config.BulletCollisionRadius));
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