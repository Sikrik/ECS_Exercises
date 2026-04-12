// 路径: Assets/Scripts/ECS/Factories/EnemyFactory.cs
using UnityEngine;

public static class EnemyFactory 
{
    public static Entity Create(EnemyType type, Vector3 spawnPos) 
    {
        var ecs = ECSManager.Instance;
        string recipeId = type.ToString();
        
        if (!ecs.Config.EnemyRecipes.TryGetValue(recipeId, out var recipe)) 
        {
            Debug.LogError($"[EnemyFactory] 找不到 ID 为 {recipeId} 的敌人配置！");
            return null;
        }

        GameObject prefab = GameObject_PoolManager.Instance.GetEnemyPrefab(type);
        GameObject go = GameObject_PoolManager.Instance.Spawn(prefab, spawnPos, Quaternion.identity);

        if (go == null) return null;

        Entity enemy = ecs.CreateEntity();
    
        // --- 核心基础组件 ---
        enemy.AddComponent(new EnemyTag());
        enemy.AddComponent(new ViewComponent(go, prefab));
        enemy.AddComponent(new PositionComponent(spawnPos.x, spawnPos.y, 0));
        enemy.AddComponent(new VelocityComponent(0, 0));
        float speedVariation = UnityEngine.Random.Range(0.85f, 1.15f);
        enemy.AddComponent(new SpeedComponent(recipe.Speed * speedVariation));
        enemy.AddComponent(new HealthComponent(recipe.Health));
        enemy.AddComponent(new DamageComponent(recipe.Damage));

        // --- 数值与战斗配置组件 ---
        enemy.AddComponent(new BountyComponent(recipe.EnemyDeathScore));
        enemy.AddComponent(new HitRecoveryStatsComponent(recipe.HitRecoveryDuration));
        enemy.AddComponent(new BounceForceComponent(recipe.BounceForce));
        
        enemy.AddComponent(new ImpactFeedbackComponent(bounce: true, recovery: true));
        enemy.AddComponent(new FactionComponent(FactionType.Enemy));
        
        enemy.AddComponent(new NeedsPhysicsBakingTag());
        enemy.AddComponent(new NeedsVisualBakingTag());
        enemy.AddComponent(new MassComponent(recipe.Health)); 

        enemy.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Player", "Enemy")));
        
        // 4. 特性装载
        if (recipe.Traits != null)
        {
            foreach (var trait in recipe.Traits) 
                ComponentRegistry.Apply(enemy, trait);
        }
        
        // 5. 特定怪物类型的额外逻辑装配
        
        if (type == EnemyType.Charger)
        {
            enemy.AddComponent(new DashAbilityComponent(recipe.SkillSpeed, recipe.SkillDuration, recipe.SkillCD));
            enemy.AddComponent(new ChargerAIComponent(recipe.ActionDist1));
        }

        if (type == EnemyType.Ranged)
        {
            // 👇 【修复 6】通过获取组件来修改数值，而不是重新 AddComponent 产生 GC 并覆盖 ComponentRegistry 的底层数据
            var weapon = enemy.GetComponent<WeaponComponent>();
            if (weapon != null) weapon.FireRate = recipe.FireRate;

            var rangedAI = enemy.GetComponent<RangedAIComponent>();
            if (rangedAI != null)
            {
                rangedAI.PreferredDistance = recipe.ActionDist1;
                rangedAI.Tolerance = recipe.ActionDist2;
                rangedAI.AttackRange = recipe.ActionDist3;
                rangedAI.PrepDuration = recipe.ActionTime1;
            }
        }

        // 6. 挂载通用方向指示器
        var indicatorView = go.GetComponent<DirectionIndicatorView>();
        if (indicatorView != null && indicatorView.ArrowPivot != null)
        {
            enemy.AddComponent(new DirectionIndicatorComponent(indicatorView.ArrowPivot, 3f));
        }

        return enemy;
    }
}