using UnityEngine;

public static class EnemyFactory 
{
    public static Entity Create(EnemyType type, Vector3 spawnPos) 
    {
        var ecs = ECSManager.Instance;
        
        // 1. 获取对应的配置配方 (从经过优化的 Enemy_config.csv 加载)
        string recipeId = type.ToString();
        if (!ecs.Config.EnemyRecipes.TryGetValue(recipeId, out var recipe)) 
        {
            Debug.LogError($"未找到敌人配置: {recipeId}");
            return null;
        }

        // 2. 表现层：从对象池获取预制体并生成
        GameObject prefab = PoolManager.Instance.GetEnemyPrefab(type);
        GameObject go = PoolManager.Instance.Spawn(prefab, spawnPos, Quaternion.identity);

        // 3. 逻辑层：创建实体并使用扩展方法挂载基础组件
        // AsEnemy 挂载 EnemyTag; WithBaseView 挂载位置、速度、视图及物理烘焙标记
        Entity enemy = ecs.CreateEntity()
            .AsEnemy()
            .WithBaseView(go, prefab, spawnPos);

        // 4. 数值组件装载 (原子化拆分)
        
        // 挂载血量组件
        enemy.AddComponent(new HealthComponent(recipe.Health));
        
        // 挂载伤害组件：让 DamageSystem 能够统一识别碰撞伤害
        enemy.AddComponent(new DamageComponent(recipe.Damage));

        // 挂载敌人状态组件：存储基础速度和硬直时间
        enemy.AddComponent(new EnemyStatsComponent 
        { 
            Type = type,
            BaseMoveSpeed = recipe.Speed, // 优化：存储基础速度以便后续效果恢复
            HitRecoveryDuration = recipe.HitRecoveryDuration, // 存储从配置读取的硬直数值
            MoveSpeed = recipe.Speed
        });

        // 5. 特性装载：根据配置中的 Traits 字符串动态挂载组件 (如 Bouncy, Ranged)
        foreach (var trait in recipe.Traits) 
        {
            ComponentRegistry.Apply(enemy, trait);
        }

        return enemy;
    }
}