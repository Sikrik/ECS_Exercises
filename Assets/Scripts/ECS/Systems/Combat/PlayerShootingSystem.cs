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
        
        // 1. 直接从配置字典中抓取当前子弹的配方数据
        string bulletId = CurrentBulletType.ToString();
        if (!config.BulletRecipes.TryGetValue(bulletId, out var bulletData)) 
        {
            Debug.LogError($"未找到子弹配置: {bulletId}");
            return;
        }

        _shootTimer += deltaTime;
        
        // 2. 直接读取配表里的 ShootInterval
        if (_shootTimer >= bulletData.ShootInterval)
        {
            // 3. 把当前子弹的具体配方 (bulletData) 传给开火逻辑
            if (Shoot(bulletData)) // 只有成功找到敌人并射击才重置计时器
            {
                _shootTimer = 0;
            }
        }
    }

    private bool Shoot(BulletData recipe)
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
        
        // 👇 核心替换：使用专属子弹配方里的通用参数
        bullet.AddComponent(new VelocityComponent(dir.x * recipe.Speed, dir.y * recipe.Speed));
        bullet.AddComponent(new LifetimeComponent { Duration= recipe.LifeTime });
        bullet.AddComponent(new DamageComponent(recipe.Damage));
        
        bullet.AddComponent(new ViewComponent(bulletGo, prefab));
        bullet.AddComponent(new NeedsBakingTag()); 
        bullet.AddComponent(new TraceComponent(pPos.X, pPos.Y));
        bullet.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Enemy")));

        // 👇 核心替换：根据特殊子弹挂载特效组件，使用配表里的特殊参数
        switch (CurrentBulletType)
        {
            case BulletType.Slow:
                bullet.AddComponent(new SlowEffectComponent(recipe.SlowRatio, recipe.SlowDuration));
                break;
            case BulletType.ChainLightning:
                // 闪电链的伤害在这里统一使用配表里的基础 Damage 字段
                bullet.AddComponent(new ChainComponent(recipe.ChainTargets, recipe.ChainRange, recipe.Damage));
                break;
            case BulletType.AOE:
                // 爆炸范围伤害也统一使用配表里的基础 Damage 字段
                bullet.AddComponent(new AOEComponent(recipe.AOERadius, recipe.Damage));
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
}