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
            if (Shoot(config)) // 只有成功找到敌人并射击才重置计时器
            {
                _shootTimer = 0;
            }
        }
    }

    private bool Shoot(GameConfig config)
    {
        var ecs = ECSManager.Instance;
        var player = ecs.PlayerEntity;
        if (player == null || !player.IsAlive) return false;

        var pPos = player.GetComponent<PositionComponent>();
        Entity target = FindNearestInGrid(pPos.X, pPos.Y, 3); // 扩大索敌深度
        if (target == null) return false;

        var tPos = target.GetComponent<PositionComponent>();
        Vector2 dir = new Vector2(tPos.X - pPos.X, tPos.Y - pPos.Y).normalized;

        GameObject prefab = PoolManager.Instance.GetBulletPrefab(CurrentBulletType);
        if (prefab == null) return false;

        GameObject bulletGo = PoolManager.Instance.Spawn(prefab, new Vector3(pPos.X, pPos.Y, 0), Quaternion.identity);

        Entity bullet = ecs.CreateEntity();
        bullet.AddComponent(new BulletTag());
        bullet.AddComponent(new PositionComponent(pPos.X, pPos.Y, 0));
        bullet.AddComponent(new VelocityComponent(dir.x * config.BulletSpeed, dir.y * config.BulletSpeed));
        bullet.AddComponent(new LifetimeComponent { RemainingTime = config.BulletLifeTime });
        bullet.AddComponent(new ViewComponent(bulletGo, prefab));
        bullet.AddComponent(new NeedsBakingTag()); 
        bullet.AddComponent(new TraceComponent(pPos.X, pPos.Y));
        bullet.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Enemy")));

        // --- 核心修复：所有子弹都必须有基础伤害组件，否则 DamageSystem 会忽略碰撞 ---
        bullet.AddComponent(new DamageComponent(config.BulletDamage));

        switch (CurrentBulletType)
        {
            case BulletType.Slow:
                bullet.AddComponent(new SlowEffectComponent(config.SlowRatio, config.SlowDuration));
                break;
            case BulletType.ChainLightning:
                bullet.AddComponent(new ChainComponent(config.ChainTargets, config.ChainRange, config.ChainDamage));
                break;
            case BulletType.AOE:
                bullet.AddComponent(new AOEComponent(config.AOERadius, config.AOEDamage));
                break;
        }

        return true;
    }

    private Entity FindNearestInGrid(float x, float y, int radius)
    {
        if (_grid == null) return null;
        var enemies = _grid.GetNearbyEnemies(x, y, radius);
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