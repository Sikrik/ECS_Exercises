// 路径: Assets/Scripts/ECS/Factories/BulletFactory.cs
using UnityEngine;

public static class BulletFactory 
{
    public static Entity Create(BulletType type, Vector3 position, Vector2 direction, FactionType sourceFaction = FactionType.Player)
    {
        var ecs = ECSManager.Instance;
        string bulletId = type.ToString();
        if (!ecs.Config.BulletRecipes.TryGetValue(bulletId, out var recipe)) return null;

        GameObject prefab = GameObject_PoolManager.Instance.GetBulletPrefab(type);
        GameObject bulletGo = GameObject_PoolManager.Instance.Spawn(prefab, position, Quaternion.identity);

        Entity bullet = ecs.CreateEntity();
        
        // --- 1. 基础组件装配 ---
        bullet.AddComponent(new BulletTag());
        bullet.AddComponent(new FactionComponent(sourceFaction));
        bullet.AddComponent(new PositionComponent(position.x, position.y, 0));
        bullet.AddComponent(new VelocityComponent(direction.x * recipe.Speed, direction.y * recipe.Speed));
        bullet.AddComponent(new ViewComponent(bulletGo, prefab));
        bullet.AddComponent(new DamageComponent(recipe.Damage));
        bullet.AddComponent(new LifetimeComponent { Duration = recipe.LifeTime });
        bullet.AddComponent(new TraceComponent(position.x, position.y));
        
        // --- 2. 补充通用物理组件 ---
        // 【核心修改】：移除硬编码的 0.2f，改为读取配方中的 HitRadius
        bullet.AddComponent(new CollisionComponent(recipe.HitRadius));
        bullet.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Enemy", "Player"))); 
        bullet.AddComponent(new NeedsPhysicsBakingTag());
        bullet.AddComponent(new NeedsVisualBakingTag());

        // 赋予子弹受击反馈组件，bounce=false(不产生物理推力)，recovery=true(触发怪物受击闪烁硬直)
        bullet.AddComponent(new ImpactFeedbackComponent(bounce: false, recovery: true));

        // --- 3. 动态挂载特殊能力 ---
        
        // 挂载减速能力
        if (recipe.SlowRatio > 0)
        {
            bullet.AddComponent(new SlowEffectComponent(recipe.SlowRatio, recipe.SlowDuration));
        }

        // 挂载闪电链能力
        if (recipe.ChainTargets > 0)
        {
            bullet.AddComponent(new ChainComponent(recipe.ChainTargets, recipe.ChainRange));
        }

        // 挂载范围爆炸能力
        if (recipe.AOERadius > 0)
        {
            bullet.AddComponent(new AOEComponent(recipe.AOERadius));
        }

        return bullet;
    }
}