using UnityEngine;

public static class EnemyFactory 
{
    public static Entity Create(EnemyType type, int level, Vector3 spawnPos) 
    {
        var ecs = ECSManager.Instance;
        // 【关键修复】从专职管理业务状态的 BattleManager 中获取配置！
        var config = BattleManager.Instance.Config; 
        
        string recipeId = $"{type.ToString()}_{level}"; 
        
        if (!config.EnemyRecipes.TryGetValue(recipeId, out var recipe)) 
        {
            Debug.LogError($"[EnemyFactory] 找不到 ID 为 {recipeId} 的敌人配置！请检查 CSV 中是否配置了该怪物等级！");
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

        // ==========================================
        // 直接使用配置表里的独立数值，不再计算全局成长
        // ==========================================
        float finalHp = recipe.Health;
        int finalDmg = recipe.Damage;
        float finalSpeed = recipe.Speed;

        float speedVariation = UnityEngine.Random.Range(0.85f, 1.15f);
        enemy.AddComponent(new SpeedComponent(finalSpeed * speedVariation));
        enemy.AddComponent(new HealthComponent(finalHp));
        enemy.AddComponent(new DamageComponent(finalDmg));

        // --- 数值与战斗配置组件 ---
        enemy.AddComponent(new BountyComponent(recipe.EnemyDeathScore));
        enemy.AddComponent(new HitRecoveryStatsComponent(recipe.HitRecoveryDuration));
        enemy.AddComponent(new BounceForceComponent(recipe.BounceForce));
        
        enemy.AddComponent(new ImpactFeedbackComponent(bounce: true, recovery: true));
        enemy.AddComponent(new FactionComponent(FactionType.Enemy));
        
        enemy.AddComponent(new NeedsPhysicsBakingTag());
        enemy.AddComponent(new NeedsVisualBakingTag());
        enemy.AddComponent(new MassComponent(finalHp)); // 质量直接挂钩最终血量

        enemy.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Player", "Enemy")));
        
        // --- 特性装载 ---
        if (recipe.Traits != null)
        {
            foreach (var trait in recipe.Traits) 
                ComponentRegistry.Apply(enemy, trait);
        }
        
        // --- 特定怪物类型的额外逻辑装配 ---
        if (type == EnemyType.Charger)
        {
            enemy.AddComponent(new DashAbilityComponent(recipe.SkillSpeed, recipe.SkillDuration, recipe.SkillCD));
            enemy.AddComponent(new ChargerAIComponent(recipe.ActionDist1));
        }

        if (type == EnemyType.Ranged)
        {
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

        // --- 挂载通用方向指示器 ---
        var indicatorView = go.GetComponent<DirectionIndicatorView>();
        if (indicatorView != null && indicatorView.ArrowPivot != null)
        {
            enemy.AddComponent(new DirectionIndicatorComponent(indicatorView.ArrowPivot, 3f));
        }
        var previewView = go.GetComponent<AttackPreviewView>();
        if (previewView != null && previewView.PreviewLine != null)
        {
            // 把 LineRenderer 包装成数据组件塞给实体
            enemy.AddComponent(new AttackPreviewVisualComponent(previewView.PreviewLine));
        }
        return enemy;
    }
}