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
        
        // --- 基础组件装配 ---
        bullet.AddComponent(new BulletTag());
        bullet.AddComponent(new FactionComponent(sourceFaction));
        bullet.AddComponent(new PositionComponent(position.x, position.y, 0));
        bullet.AddComponent(new VelocityComponent(direction.x * recipe.Speed, direction.y * recipe.Speed));
        bullet.AddComponent(new ViewComponent(bulletGo, prefab));
        bullet.AddComponent(new DamageComponent(recipe.Damage));
        bullet.AddComponent(new LifetimeComponent { Duration = recipe.LifeTime });
        bullet.AddComponent(new TraceComponent(position.x, position.y));
        
        // 补充通用物理组件
        bullet.AddComponent(new CollisionComponent(0.2f));
        bullet.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Enemy", "Player"))); 
        bullet.AddComponent(new NeedsPhysicsBakingTag());
        bullet.AddComponent(new NeedsVisualBakingTag());

        // 【重构点】：使用组件注册表动态装载能力，彻底移除 switch(type)
        // 假设 recipe.Traits 存储了 "Slow", "AOE", "Chain" 等字符串数组
        if (recipe.Traits != null)
        {
            foreach (var trait in recipe.Traits) 
                ComponentRegistry.Apply(bullet, trait);
        }

        return bullet;
    }
}