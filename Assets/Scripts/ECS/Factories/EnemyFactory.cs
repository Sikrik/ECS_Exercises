// 路径: Assets/Scripts/ECS/Factories/EnemyFactory.cs
using UnityEngine;

public static class EnemyFactory 
{
    public static Entity Create(EnemyType type, Vector3 spawnPos) 
    {
        var ecs = ECSManager.Instance;
        string recipeId = type.ToString();
        
        // 1. 获取配置数据
        if (!ecs.Config.EnemyRecipes.TryGetValue(recipeId, out var recipe)) 
        {
            Debug.LogError($"[EnemyFactory] 找不到 ID 为 {recipeId} 的敌人配置！");
            return null;
        }

        // 2. 表现层实例化（使用对象池）
        GameObject prefab = GameObject_PoolManager.Instance.GetEnemyPrefab(type);
        GameObject go = GameObject_PoolManager.Instance.Spawn(prefab, spawnPos, Quaternion.identity);

        // 防御性校验：防止因面板漏填预制体导致的严重报错
        if (go == null) 
        {
            Debug.LogError($"[EnemyFactory] 实体生成失败！{type} 的预制体为空，请检查 GameObject_PoolManager 面板是否已赋值！");
            return null;
        }

        // 3. 创建逻辑层实体并挂载基础组件
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
        
        // ImpactFeedback 决定碰撞时是否产生物理反弹和受击硬直
        enemy.AddComponent(new ImpactFeedbackComponent(bounce: true, recovery: true));
        enemy.AddComponent(new FactionComponent(FactionType.Enemy));
        
        // --- 物理与视觉烘焙标记（交由对应的 BakingSystem 在第一帧处理） ---
        enemy.AddComponent(new NeedsPhysicsBakingTag());
        enemy.AddComponent(new NeedsVisualBakingTag());
        enemy.AddComponent(new MassComponent(recipe.Health)); // 质量与血量挂钩，影响挤压力度

        // 设置碰撞过滤掩码
        enemy.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Player", "Enemy")));
        
        // 4. 特性装载（数据驱动：将 CSV 中的 Trait 字符串转换为组件）
        if (recipe.Traits != null)
        {
            foreach (var trait in recipe.Traits) 
                ComponentRegistry.Apply(enemy, trait);
        }
        
        // 5. 特定怪物类型的额外逻辑装配
        
        // 冲锋怪 (Charger) 特有组件
        if (type == EnemyType.Charger)
        {
            enemy.AddComponent(new DashAbilityComponent(25f, 0.6f, 3f));
            enemy.AddComponent(new ChargerAIComponent(8f));
        }

        // 远程怪 (Ranged) 特有组件
        if (type == EnemyType.Ranged)
        {
            // 发放武器 (使用普通子弹，射击间隔 2.5 秒)
            enemy.AddComponent(new WeaponComponent(BulletType.Normal, 4f));
            
            // 装配远程 AI (射程 8 米，预警蓄力 1.0 秒)
            // 注意：需确保 RangedAIComponent 已修复包含 AttackRange 和 PrepDuration 字段
            enemy.AddComponent(new RangedAIComponent(dist: 4f, tolerance: 1f, attackRange: 8f, prepDuration: 1.0f));
        }

        // 6. 挂载通用方向指示器（表现层箭头）
        var indicatorView = go.GetComponent<DirectionIndicatorView>();
        if (indicatorView != null && indicatorView.ArrowPivot != null)
        {
            // 怪物转向阻尼较大，设为 3f
            enemy.AddComponent(new DirectionIndicatorComponent(indicatorView.ArrowPivot, 3f));
        }

        return enemy;
    }
}