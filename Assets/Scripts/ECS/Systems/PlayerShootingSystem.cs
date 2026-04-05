using System.Collections.Generic;
using UnityEngine;

public class PlayerShootingSystem : SystemBase
{
    private float _shootTimer;
    private GridSystem _grid;
    public static BulletType CurrentBulletType = BulletType.Normal;
    
    public PlayerShootingSystem(List<Entity> entities, GridSystem grid) : base(entities) 
    {
        _grid = grid;
    }
    
    public override void Update(float deltaTime)
    {
        var config = ECSManager.Instance.Config;
        float interval = CurrentBulletType switch {
            BulletType.Slow => config.SlowBulletShootInterval,
            BulletType.ChainLightning => config.ChainLightningShootInterval,
            BulletType.AOE => config.AOEBulletShootInterval,
            _ => config.ShootInterval
        };
        
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
        
        // 1. 空间优化：只在网格中寻找最近目标
        Entity target = FindNearestInGrid(pPos.X, pPos.Y);
        if (target == null) return;
        
        var tPos = target.GetComponent<PositionComponent>();
        Vector2 dir = new Vector2(tPos.X - pPos.X, tPos.Y - pPos.Y).normalized;
        
        // 2. 生成物理对象
        GameObject prefab = PoolManager.Instance.GetBulletPrefab(CurrentBulletType);
        GameObject bulletGo = PoolManager.Instance.Spawn(prefab, new Vector3(pPos.X, pPos.Y, 0), Quaternion.identity);
        
        // 3. 创建实体
        Entity bullet = ecs.CreateEntity();
        bullet.AddComponent(new PositionComponent(pPos.X, pPos.Y, 0));
        bullet.AddComponent(new VelocityComponent(dir.x * config.BulletSpeed, dir.y * config.BulletSpeed, 0));
        bullet.AddComponent(new ViewComponent(bulletGo));
        bullet.AddComponent(new BulletComponent { Type = CurrentBulletType });

        // 4. 核心架构重构：根据子弹类型组合“能力组件”
        // 这样 BulletEffectSystem 就不需要写 switch-case 了
        switch(CurrentBulletType)
        {
            case BulletType.Normal:
                bullet.AddComponent(new DamageComponent(config.BulletDamage));
                break;
            case BulletType.AOE:
                bullet.AddComponent(new AOEComponent(config.AOEBulletRadius, config.AOEBulletDamage));
                break;
            case BulletType.ChainLightning:
                bullet.AddComponent(new DamageComponent(config.BulletDamage)); // 初始命中伤害
                bullet.AddComponent(new ChainComponent(config.ChainLightningMaxTargets, config.ChainLightningChainRange, config.ChainLightningDamage));
                break;
        }

        // 5. 同步半径
        float radius = config.BulletCollisionRadius;
        if (bulletGo.TryGetComponent<SpriteRenderer>(out var sr))
            radius = Mathf.Min(sr.bounds.size.x, sr.bounds.size.y) * 0.5f;
        bullet.AddComponent(new CollisionComponent(radius));
    }
    
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