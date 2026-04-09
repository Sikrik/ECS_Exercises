using UnityEngine;

/// <summary>
/// 子弹工厂：负责根据配置创建不同类型的子弹实体
/// </summary>
public static class BulletFactory 
{
    public static Entity Create(BulletType type, Vector3 position, Vector2 direction)
    {
        var ecs = ECSManager.Instance;
        var config = ecs.Config;
        
        // 1. 获取子弹配置数据
        string bulletId = type.ToString();
        if (!config.BulletRecipes.TryGetValue(bulletId, out var recipe)) 
        {
            Debug.LogError($"[BulletFactory] 未找到子弹配置: {bulletId}");
            return null;
        }

        // 2. 实例化视觉表现层 (从对象池获取)
        GameObject prefab = GameObject_PoolManager.Instance.GetBulletPrefab(type);
        if (prefab == null) return null;
        GameObject bulletGo = GameObject_PoolManager.Instance.Spawn(prefab, position, Quaternion.identity);

        // 3. 创建 ECS 实体并组装基础组件
        Entity bullet = ecs.CreateEntity();
        bullet.AddComponent(new BulletTag());
        bullet.AddComponent(new PositionComponent(position.x, position.y, 0));
        bullet.AddComponent(new VelocityComponent(direction.x * recipe.Speed, direction.y * recipe.Speed));
        bullet.AddComponent(new ViewComponent(bulletGo, prefab));
        
        // 4. 组装战斗与物理基础组件
        bullet.AddComponent(new DamageComponent(recipe.Damage));
        bullet.AddComponent(new LifetimeComponent { Duration = recipe.LifeTime });
        bullet.AddComponent(new CollisionComponent(0.2f));
        bullet.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Enemy")));
        
        // 5. 关键标记：触发后续的物理烘焙 (PhysicsBakingSystem)
        bullet.AddComponent(new NeedsBakingTag());
        bullet.AddComponent(new TraceComponent(position.x, position.y));

        // 6. 根据子弹类型装载特殊效果组件 (原子化配置)
        switch (type)
        {
            case BulletType.Slow:
                // 仅存储减速参数，不存储伤害
                bullet.AddComponent(new SlowEffectComponent(recipe.SlowRatio, recipe.SlowDuration)); 
                break;
            case BulletType.ChainLightning:
                // 仅存储连锁参数
                bullet.AddComponent(new ChainComponent(recipe.ChainTargets, recipe.ChainRange));
                break;
            case BulletType.AOE:
                // 仅存储爆炸半径
                bullet.AddComponent(new AOEComponent(recipe.AOERadius));
                break;
        }

        return bullet;
    }
}