// 路径: Assets/Scripts/ECS/Factories/EnemyFactory.cs
using UnityEngine;

public static class EnemyFactory 
{
    public static Entity Create(EnemyType type, Vector3 spawnPos) 
    {
        var ecs = ECSManager.Instance;
        string recipeId = type.ToString();
        if (!ecs.Config.EnemyRecipes.TryGetValue(recipeId, out var recipe)) return null;

        GameObject prefab = GameObject_PoolManager.Instance.GetEnemyPrefab(type);
        GameObject go = GameObject_PoolManager.Instance.Spawn(prefab, spawnPos, Quaternion.identity);

        // 防御性校验：防止因面板漏填预制体导致的严重报错
        if (go == null) 
        {
            Debug.LogError($"[EnemyFactory] 实体生成失败！{type} 的预制体为空，请检查 GameObject_PoolManager 面板是否已赋值！");
            return null;
        }

        Entity enemy = ecs.CreateEntity();
    
        // --- 基础组件 ---
        enemy.AddComponent(new EnemyTag());
        enemy.AddComponent(new ViewComponent(go, prefab));
        enemy.AddComponent(new PositionComponent(spawnPos.x, spawnPos.y, 0));
        enemy.AddComponent(new VelocityComponent(0, 0));
        enemy.AddComponent(new SpeedComponent(recipe.Speed)); 
        enemy.AddComponent(new HealthComponent(recipe.Health)); 
        enemy.AddComponent(new DamageComponent(recipe.Damage)); 

        // --- 原子化配置组件 ---
        enemy.AddComponent(new BountyComponent(recipe.EnemyDeathScore)); 
        enemy.AddComponent(new HitRecoveryStatsComponent(recipe.HitRecoveryDuration));
        enemy.AddComponent(new BounceForceComponent(recipe.BounceForce));
        
        enemy.AddComponent(new ImpactFeedbackComponent(bounce: true, recovery: true));
        enemy.AddComponent(new FactionComponent(FactionType.Enemy));
        
        // --- 特性装载 ---
        enemy.AddComponent(new NeedsPhysicsBakingTag());
        enemy.AddComponent(new NeedsVisualBakingTag());
        enemy.AddComponent(new MassComponent(recipe.Health)); 

        enemy.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Player", "Enemy")));
        
        if (recipe.Traits != null)
        {
            foreach (var trait in recipe.Traits) 
                ComponentRegistry.Apply(enemy, trait);
        }
        
        // ==========================================
        // 冲锋怪能力装配
        // ==========================================
        if (type.ToString() == "Charger")
        {
            enemy.AddComponent(new DashAbilityComponent(25f, 0.6f, 3f));
            enemy.AddComponent(new ChargerAIComponent(8f));
        }

        // ==========================================
        // 远程怪能力装配
        // ==========================================
        if (type.ToString() == "Ranged")
        {
            // 发放武器 (打普通子弹，射击真正的CD设为 2.5 秒)
            enemy.AddComponent(new WeaponComponent(BulletType.Normal, 2.5f));
            
            // 装配远程 AI (射程 8 米，开火前摇红外线蓄力 1.0 秒)
            enemy.AddComponent(new RangedAIComponent(8f, 1.0f));
        }

        // ==========================================
        // 挂载通用方向指示器
        // ==========================================
        var indicatorView = go.GetComponent<DirectionIndicatorView>();
        if (indicatorView != null && indicatorView.ArrowPivot != null)
        {
            // 怪物转向阻尼更大（转得慢），设为 3f
            enemy.AddComponent(new DirectionIndicatorComponent(indicatorView.ArrowPivot, 3f));
        }

        return enemy;
    }
}