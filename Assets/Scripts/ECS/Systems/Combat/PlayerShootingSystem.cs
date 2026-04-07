using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家射击系统：负责根据当前选择的子弹类型进行开火逻辑处理
/// 重构点：统一使用 DamageComponent 存储破坏力，特殊效果组件仅存储范围参数
/// </summary>
public class PlayerShootingSystem : SystemBase
{
    private float _shootTimer;
    private GridSystem _grid;
    public static BulletType CurrentBulletType = BulletType.Normal;
    
    public PlayerShootingSystem(List<Entity> entities, GridSystem grid) : base(entities) { _grid = grid; }
    
    public override void Update(float deltaTime)
    {
        var config = ECSManager.Instance.Config;
        
        // 1. 获取当前子弹类型的配置数据
        string bulletId = CurrentBulletType.ToString();
        if (!config.BulletRecipes.TryGetValue(bulletId, out var bulletData)) 
        {
            Debug.LogError($"未找到子弹配置: {bulletId}");
            return;
        }

        _shootTimer += deltaTime;
        
        // 2. 检查射击间隔
        if (_shootTimer >= bulletData.ShootInterval)
        {
            if (Shoot(bulletData)) 
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
        Entity target = FindNearestInGrid(pPos.X, pPos.Y, 3); 
        if (target == null) return false;

        var tPos = target.GetComponent<PositionComponent>();
        Vector2 dir = new Vector2(tPos.X - pPos.X, tPos.Y - pPos.Y).normalized;

        GameObject prefab = GameObject_PoolManager.Instance.GetBulletPrefab(CurrentBulletType);
        if (prefab == null) return false;

        GameObject bulletGo = GameObject_PoolManager.Instance.Spawn(prefab, new Vector3(pPos.X, pPos.Y, 0), Quaternion.identity);

        Entity bullet = ecs.CreateEntity();
        bullet.AddComponent(new BulletTag());
        bullet.AddComponent(new PositionComponent(pPos.X, pPos.Y, 0));
        
        // ==========================================
        // 核心逻辑：原子化组件装载
        // ==========================================
        // 无论何种类型的子弹，伤害数值只在这里存一次
        bullet.AddComponent(new DamageComponent(recipe.Damage));
        bullet.AddComponent(new VelocityComponent(dir.x * recipe.Speed, dir.y * recipe.Speed));
        bullet.AddComponent(new LifetimeComponent { Duration = recipe.LifeTime });
        
        bullet.AddComponent(new ViewComponent(bulletGo, prefab));
        bullet.AddComponent(new NeedsBakingTag()); 
        bullet.AddComponent(new TraceComponent(pPos.X, pPos.Y));
        bullet.AddComponent(new CollisionComponent(0.2f));
        bullet.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Enemy")));

        // 根据类型挂载特殊效果组件（不再传入 Damage 参数）
        switch (CurrentBulletType)
        {
            case BulletType.Slow:
                bullet.AddComponent(new SlowEffectComponent(recipe.SlowRatio, recipe.SlowDuration));
                break;
            case BulletType.ChainLightning:
                bullet.AddComponent(new ChainComponent(recipe.ChainTargets, recipe.ChainRange));
                break;
            case BulletType.AOE:
                bullet.AddComponent(new AOEComponent(recipe.AOERadius));
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