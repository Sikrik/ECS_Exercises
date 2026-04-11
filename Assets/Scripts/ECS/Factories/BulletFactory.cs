using UnityEngine;

/// <summary>
/// 子弹工厂：负责根据配置创建不同类型的子弹实体
/// </summary>
public static class BulletFactory 
{
    // 【核心重构 1】：增加 FactionType 参数，默认是 Player 发射的子弹
    // 未来添加会远程攻击的怪物时，只需传入 FactionType.Enemy 即可！
    public static Entity Create(BulletType type, Vector3 position, Vector2 direction, FactionType sourceFaction = FactionType.Player)
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
        
        // --- 基础身份与阵营 ---
        bullet.AddComponent(new BulletTag());
        bullet.AddComponent(new FactionComponent(sourceFaction)); // 【核心重构 2】：赋予子弹所属阵营
        
        // --- 运动与表现 ---
        bullet.AddComponent(new PositionComponent(position.x, position.y, 0));
        bullet.AddComponent(new VelocityComponent(direction.x * recipe.Speed, direction.y * recipe.Speed));
        bullet.AddComponent(new ViewComponent(bulletGo, prefab));
        
        // --- 伤害与寿命 ---
        bullet.AddComponent(new DamageComponent(recipe.Damage));
        bullet.AddComponent(new LifetimeComponent { Duration = recipe.LifeTime });
        
        // --- 物理防穿透 ---
        bullet.AddComponent(new CollisionComponent(0.2f));
        // 这里可以允许子弹检测所有实体，真正的免伤过滤在 DamageSystem 里通过阵营判断
        bullet.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Enemy", "Player"))); 
        bullet.AddComponent(new NeedsPhysicsBakingTag());
        bullet.AddComponent(new NeedsVisualBakingTag());
        bullet.AddComponent(new TraceComponent(position.x, position.y));

        // ==========================================
        // 【核心重构 3】：差异化装载特殊能力
        // 彻底移除了 ImpactFeedbackComponent，因为我们遵循业务规则：子弹命中不产生物理反弹
        // ==========================================
        switch (type)
        {
            case BulletType.Normal:
                // 普通子弹：纯扣血，无附加状态
                break;
                
            case BulletType.Slow:
                // 冰冻弹：依然挂载减速纯逻辑组件
                bullet.AddComponent(new SlowEffectComponent(recipe.SlowRatio, recipe.SlowDuration)); 
                break;
                
            case BulletType.ChainLightning:
                // 闪电弹：挂载闪电链参数
                bullet.AddComponent(new ChainComponent(recipe.ChainTargets, recipe.ChainRange));
                break;
                
            case BulletType.AOE:
                // 爆炸弹：挂载范围参数
                bullet.AddComponent(new AOEComponent(recipe.AOERadius));
                break;
        }

        return bullet;
    }
}