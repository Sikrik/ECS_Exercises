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
            // 核心修复：只有成功射击后才清空计时器
            // 这样如果范围内没敌人，计时器会保持在就绪状态，一旦有敌人进入射程立即开火
            if (Shoot(config)) 
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
        
        // 核心修复：扩大网格搜索深度（从 1 改为 3），确保能打到屏幕边缘的敌人
        Entity target = FindNearestInGrid(pPos.X, pPos.Y, 3); 
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

        // 分发子弹效果
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

        return true;
    }

    private Entity FindNearestInGrid(float x, float y, int radius)
    {
        if (_grid == null) return null;
        // 使用带半径参数的索敌方法
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