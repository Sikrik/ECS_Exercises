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
        
        string bulletId = type.ToString();
        if (!config.BulletRecipes.TryGetValue(bulletId, out var recipe)) 
        {
            Debug.LogError($"[BulletFactory] 未找到子弹配置: {bulletId}");
            return null;
        }

        GameObject prefab = GameObject_PoolManager.Instance.GetBulletPrefab(type);
        if (prefab == null) return null;
        GameObject bulletGo = GameObject_PoolManager.Instance.Spawn(prefab, position, Quaternion.identity);

        Entity bullet = ecs.CreateEntity();
        bullet.AddComponent(new BulletTag());
        bullet.AddComponent(new PositionComponent(position.x, position.y, 0));
        bullet.AddComponent(new VelocityComponent(direction.x * recipe.Speed, direction.y * recipe.Speed));
        bullet.AddComponent(new ViewComponent(bulletGo, prefab));
        
        bullet.AddComponent(new DamageComponent(recipe.Damage));
        bullet.AddComponent(new LifetimeComponent { Duration = recipe.LifeTime });
        bullet.AddComponent(new CollisionComponent(0.2f));
        bullet.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Enemy")));
        
        bullet.AddComponent(new NeedsBakingTag());
        bullet.AddComponent(new TraceComponent(position.x, position.y));

        // ==========================================
        // 核心重构：差异化装载碰撞反馈意图 (ImpactFeedbackComponent)
        // ==========================================
        switch (type)
        {
            case BulletType.Normal:
                // 方案三：普通子弹，打中后既物理击退，又造成硬直
                bullet.AddComponent(new ImpactFeedbackComponent(bounce: true, recovery: true));
                break;
                
            case BulletType.Slow:
                bullet.AddComponent(new SlowEffectComponent(recipe.SlowRatio, recipe.SlowDuration)); 
                // 冰冻弹：依然有物理冲击和硬直
                bullet.AddComponent(new ImpactFeedbackComponent(bounce: true, recovery: true));
                break;
                
            case BulletType.ChainLightning:
                bullet.AddComponent(new ChainComponent(recipe.ChainTargets, recipe.ChainRange));
                // 方案特殊：闪电只有触电硬直（麻痹），没有物理击退（无质量）
                bullet.AddComponent(new ImpactFeedbackComponent(bounce: false, recovery: true));
                break;
                
            case BulletType.AOE:
                bullet.AddComponent(new AOEComponent(recipe.AOERadius));
                // 方案二：爆炸核心可能产生极强的击退，但为了防止怪物连续罚站，可能不给硬直
                bullet.AddComponent(new ImpactFeedbackComponent(bounce: true, recovery: false));
                break;
        }

        return bullet;
    }
}