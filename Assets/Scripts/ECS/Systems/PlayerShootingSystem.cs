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

    private void Shoot(GameConfig config)
    {
        var ecs = ECSManager.Instance;
        var player = ecs.PlayerEntity;
        if (player == null || !player.IsAlive) return;

        var pPos = player.GetComponent<PositionComponent>();
        Entity target = FindNearestInGrid(pPos.X, pPos.Y);
        if (target == null) return;

        var tPos = target.GetComponent<PositionComponent>();
        Vector2 dir = new Vector2(tPos.X - pPos.X, tPos.Y - pPos.Y).normalized;

        GameObject prefab = PoolManager.Instance.GetBulletPrefab(CurrentBulletType);
        if (prefab == null) return;

        GameObject bulletGo = PoolManager.Instance.Spawn(prefab, new Vector3(pPos.X, pPos.Y, 0), Quaternion.identity);

        Entity bullet = ecs.CreateEntity();
        bullet.AddComponent(new BulletTag());
        bullet.AddComponent(new PositionComponent(pPos.X, pPos.Y, 0));
        bullet.AddComponent(new VelocityComponent(dir.x * config.BulletSpeed, dir.y * config.BulletSpeed));
        bullet.AddComponent(new LifetimeComponent { RemainingTime = config.BulletLifeTime });
        bullet.AddComponent(new ViewComponent(bulletGo, prefab));
        
        // --- 核心修复：不再手动加 CollisionComponent，交给烘焙系统 ---
        bullet.AddComponent(new NeedsBakingTag()); 
        bullet.AddComponent(new TraceComponent(pPos.X, pPos.Y));

        // 根据子弹类型分发效果（保持不变）
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
                bullet.AddComponent(new ChainComponent(config.ChainTargets, config.ChainRange, config.ChainDamage));
                break;
            case BulletType.AOE:
                bullet.AddComponent(new AOEComponent(config.AOERadius, config.AOEDamage));
                break;
        }
    }

    private Entity FindNearestInGrid(float x, float y)
    {
        if (_grid == null) return null;
        var enemies = _grid.GetNearbyEnemies(x, y);
        Entity nearest = null;
        float minDistSq = float.MaxValue;
        foreach (var e in enemies)
        {
            if (!e.IsAlive || !e.HasComponent<EnemyTag>()) continue;
            var ePos = e.GetComponent<PositionComponent>();
            float d2 = (ePos.X - x) * (ePos.X - x) + (ePos.Y - y) * (ePos.Y - y);
            if (d2 < minDistSq) { minDistSq = d2; nearest = e; }
        }
        return nearest;
    }

    private float GetInterval(GameConfig config) => CurrentBulletType switch {
        BulletType.Slow => config.SlowBulletShootInterval,
        BulletType.ChainLightning => config.ChainLightningShootInterval,
        BulletType.AOE => config.AOEBulletShootInterval,
        _ => config.ShootInterval
    };
}