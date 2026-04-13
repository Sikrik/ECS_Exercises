using UnityEngine;

public static class BulletFactory 
{
    public static Entity Create(BulletType type, Vector3 position, Vector2 direction, FactionType sourceFaction = FactionType.Player, WeaponModifierComponent modifiers = null)
    {
        var ecs = ECSManager.Instance;
        
        // 【提取动态配置】
        if (!ecs.Config.BulletRecipes.TryGetValue(type.ToString(), out var recipe))
        {
            Debug.LogError($"[BulletFactory] 找不到子弹配置：{type}，请检查 Bullet_Config.csv");
            return null;
        }

        GameObject prefab = GameObject_PoolManager.Instance.GetBulletPrefab(type);
        GameObject bulletGo = GameObject_PoolManager.Instance.Spawn(prefab, position, Quaternion.identity);

        Entity bullet = ecs.CreateEntity();
        
        // ==========================================
        // 基础默认数值 (已取消硬编码，全数由CSV提供)
        // ==========================================
        float baseSpeed = recipe.Speed;
        float baseDamage = recipe.Damage;
        float baseHitRadius = recipe.HitRadius;
        float baseLifeTime = recipe.LifeTime;

        // --- 1. 基础组件装配 ---
        bullet.AddComponent(new BulletTag());
        bullet.AddComponent(new FactionComponent(sourceFaction));
        bullet.AddComponent(new PositionComponent(position.x, position.y, 0));
        bullet.AddComponent(new VelocityComponent(direction.x * baseSpeed, direction.y * baseSpeed));
        bullet.AddComponent(new ViewComponent(bulletGo, prefab));
        bullet.AddComponent(new LifetimeComponent { Duration = baseLifeTime });
        bullet.AddComponent(new TraceComponent(position.x, position.y));
        
        // --- 2. 补充通用物理组件 ---
        bullet.AddComponent(new CollisionComponent(baseHitRadius));
        bullet.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Enemy", "Player"))); 
        bullet.AddComponent(new NeedsPhysicsBakingTag());
        bullet.AddComponent(new NeedsVisualBakingTag());

        // ==========================================
        // 3. 动态结算局内与局外的伤害修饰系统
        // ==========================================
        float finalDamage = baseDamage;
        bool causeRecovery = false;       
        float stunDurationOverride = 0f;  

        if (modifiers != null)
        {
            // 【核心计算】：应用局外天赋带来的全局攻击倍率
            finalDamage *= modifiers.GlobalDamageMultiplier;

            // 应用局内局内卡牌/升级带来的火力强化 (每级+20%)
            int attackLevel = modifiers.GetLevel("AttackUp");
            finalDamage *= (1f + attackLevel * 0.2f);

            // 附魔：冰霜减速
            if (modifiers.GetLevel("AddSlow") > 0)
            {
                int slowEnhance = modifiers.GetLevel("SlowEnhance");
                float ratio = Mathf.Min(0.5f + slowEnhance * 0.08f, 0.9f); 
                float duration = 2f + slowEnhance * 0.5f;
                bullet.AddComponent(new SlowEffectComponent(ratio, duration));
            }
                
            // 附魔：闪电链
            if (modifiers.GetLevel("AddChain") > 0)
            {
                int chainEnhance = modifiers.GetLevel("ChainEnhance");
                int targets = 3 + chainEnhance * 2;          
                float range = 5f + chainEnhance * 1.5f;      
                bullet.AddComponent(new ChainComponent(targets, range));
            }
                
            // 附魔：范围爆裂
            if (modifiers.GetLevel("AddAOE") > 0)
            {
                int aoeEnhance = modifiers.GetLevel("AOEEnhance");
                float radius = 2f + aoeEnhance * 0.8f;       
                bullet.AddComponent(new AOEComponent(radius));
            }

            // 附魔：重击硬直
            if (modifiers.GetLevel("AddStun") > 0)
            {
                causeRecovery = true;
                int stunEnhance = modifiers.GetLevel("StunEnhance");
                stunDurationOverride = 0.2f + stunEnhance * 0.15f; 
            }
        }

        // 挂载物理反馈与最终伤害配置
        bullet.AddComponent(new ImpactFeedbackComponent(bounce: false, recovery: causeRecovery, stunDurationOverride));
        bullet.AddComponent(new DamageComponent(finalDamage));

        return bullet;
    }
}