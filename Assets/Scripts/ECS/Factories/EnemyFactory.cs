using UnityEngine;

/// <summary>
/// 敌人工厂：基于组件组合模式，将配置模板转化为 ECS 实体
/// </summary>
public static class EnemyFactory
{
    public static Entity Create(EnemyType type, Vector3 spawnPos)
    {
        var ecs = ECSManager.Instance;
        var pool = PoolManager.Instance;
        var config = ecs.Config;

        // 1. 获取表现层预制体并实例化（通过对象池）
        GameObject prefab = pool.GetEnemyPrefab(type);
        if (prefab == null) return null;

        GameObject go = pool.Spawn(prefab, spawnPos, Quaternion.identity);

        // 2. 获取该类型的配置数据（这里模拟了从 GameConfig 获取数据）
        // 实际上可以进一步优化为从 Dictionary<EnemyType, EnemyTemplate> 中读取
        EnemyTemplate template = GetTemplateByType(type, config);

        // 3. 创建实体并挂载基础组件
        Entity enemy = ecs.CreateEntity();
        
        // 身份与视图
        enemy.AddComponent(new EnemyTag());
        enemy.AddComponent(new ViewComponent(go, prefab));
        
        // 物理与位置
        enemy.AddComponent(new PositionComponent(spawnPos.x, spawnPos.y, 0));
        enemy.AddComponent(new VelocityComponent(0, 0));
        enemy.AddComponent(new NeedsBakingTag()); // 标记给 PhysicsBakingSystem 处理
        
        // 碰撞过滤：怪物主要检测 Player 层
        enemy.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Player")));

        // 4. 根据模板动态组合战斗组件（核心原则：组件组合）
        enemy.AddComponent(new HealthComponent(template.Health));
        enemy.AddComponent(new DamageComponent(template.Damage));
        
        // 存储运行时数值
        enemy.AddComponent(new EnemyStatsComponent { 
            Type = type, 
            MoveSpeed = template.Speed, 
            Damage = template.Damage,
            AttackCooldown = config.EnemyAttackCooldown 
        });

        // --- 特性装配：根据模板布尔值决定是否挂载标签 ---
        if (template.IsBouncy)
        {
            enemy.AddComponent(new BouncyTag()); // 只有挂载了此标签，KnockbackSystem 才会执行反弹
        }

        return enemy;
    }

    /// <summary>
    /// 辅助方法：将分散的 GameConfig 数值映射到模板对象
    /// 未来建议将此逻辑整合进 ConfigLoader 直接从 CSV 生成 Dictionary
    /// </summary>
    private static EnemyTemplate GetTemplateByType(EnemyType type, GameConfig config)
    {
        return type switch
        {
            EnemyType.Fast => new EnemyTemplate(config.FastEnemyMaxHealth, config.FastEnemySpeed, config.EnemyDamage, true),
            EnemyType.Tank => new EnemyTemplate(config.TankEnemyMaxHealth, config.TankEnemySpeed, config.EnemyDamage, false), // 坦克不反弹
            _ => new EnemyTemplate(config.EnemyMaxHealth, config.EnemyMoveSpeed, config.EnemyDamage, true)
        };
    }
}