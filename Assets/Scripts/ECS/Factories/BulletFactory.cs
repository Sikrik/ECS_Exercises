using UnityEngine;

public static class BulletFactory 
{
    public static Entity Create(BulletType type, Vector3 position, Vector2 direction, FactionType sourceFaction = FactionType.Player, WeaponModifierComponent modifiers = null)
    {
        var ecs = ECSManager.Instance;
        
        GameObject prefab = GameObject_PoolManager.Instance.GetBulletPrefab(type);
        GameObject bulletGo = GameObject_PoolManager.Instance.Spawn(prefab, position, Quaternion.identity);

        Entity bullet = ecs.CreateEntity();
        
        // ==========================================
        // 基础默认数值
        // ==========================================
        float baseSpeed = 12f;
        float baseDamage = 15f;
        float baseHitRadius = 0.2f;
        float baseLifeTime = 3f;

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
        // 3. 动态结算升级系统修饰器 (提取深度成长数值)
        // ==========================================
        float finalDamage = baseDamage;
        bool causeRecovery = false;       // 【修改】子弹默认不再造成硬直
        float stunDurationOverride = 0f;  // 【新增】默认无硬直覆盖时间

        if (modifiers != null)
        {
            // 攻击力提升 (每级+20%)
            int attackLevel = modifiers.GetLevel("AttackUp");
            finalDamage *= (1f + attackLevel * 0.2f);

            // 减速附魔与强化
            if (modifiers.GetLevel("AddSlow") > 0)
            {
                int slowEnhance = modifiers.GetLevel("SlowEnhance");
                float ratio = Mathf.Min(0.5f + slowEnhance * 0.08f, 0.9f); // 最大90%减速
                float duration = 2f + slowEnhance * 0.5f;
                bullet.AddComponent(new SlowEffectComponent(ratio, duration));
            }
                
            // 闪电附魔与强化
            if (modifiers.GetLevel("AddChain") > 0)
            {
                int chainEnhance = modifiers.GetLevel("ChainEnhance");
                int targets = 3 + chainEnhance * 2;          // 基础弹射3次，每级+2
                float range = 5f + chainEnhance * 1.5f;      // 基础范围5米，每级+1.5
                bullet.AddComponent(new ChainComponent(targets, range));
            }
                
            // 爆炸附魔与强化
            if (modifiers.GetLevel("AddAOE") > 0)
            {
                int aoeEnhance = modifiers.GetLevel("AOEEnhance");
                float radius = 2f + aoeEnhance * 0.8f;       // 基础半径2米，每级+0.8
                bullet.AddComponent(new AOEComponent(radius));
            }

            // 【新增】硬直附魔与强化
            if (modifiers.GetLevel("AddStun") > 0)
            {
                causeRecovery = true;
                int stunEnhance = modifiers.GetLevel("StunEnhance");
                // 基础硬直 0.2 秒，每级增强 0.15 秒
                stunDurationOverride = 0.2f + stunEnhance * 0.15f; 
            }
        }

        // 挂载物理反馈意图 (带有算好的硬直配置)
        bullet.AddComponent(new ImpactFeedbackComponent(bounce: false, recovery: causeRecovery, stunDurationOverride));
        
        // 最后挂载计算后的最终伤害
        bullet.AddComponent(new DamageComponent(finalDamage));

        return bullet;
    }
}