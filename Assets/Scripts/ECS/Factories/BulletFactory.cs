// 路径: Assets/Scripts/ECS/Factories/BulletFactory.cs
using UnityEngine;

public static class BulletFactory 
{
    public static Entity Create(BulletType type, Vector3 position, Vector2 direction, FactionType sourceFaction = FactionType.Player, WeaponModifierComponent modifiers = null)
    {
        var config = BattleManager.Instance.Config;
        var ecs = ECSManager.Instance;
        
        if (!config.BulletRecipes.TryGetValue(type.ToString(), out var recipe)) return null;

        GameObject prefab = GameObject_PoolManager.Instance.GetBulletPrefab(type);
        GameObject bulletGo = GameObject_PoolManager.Instance.Spawn(prefab, position, Quaternion.identity);

        Entity bullet = ecs.CreateEntity();
        
        float baseSpeed = recipe.Speed;
        float baseDamage = recipe.Damage;
        float finalDamage = baseDamage;
        bool causeRecovery = false;       
        float stunDurationOverride = 0f;  

        // ==========================================
        // 第一阶段：处理基础属性修正（需在挂载组件前计算）
        // ==========================================
        if (modifiers != null)
        {
            finalDamage *= modifiers.GlobalDamageMultiplier;
            
            // 1. 基础伤害提升 (对应表: IncreaseDamage)
            finalDamage *= (1f + modifiers.GetLevel("IncreaseDamage") * 0.2f); // 每级提升20%

            // 2. 子弹速度提升 (对应表: IncreaseBulletSpeed)
            int speedLvl = modifiers.GetLevel("IncreaseBulletSpeed");
            if (speedLvl > 0)
            {
                baseSpeed *= (1f + speedLvl * 0.15f); // 每级提升15%飞行速度
            }
        }

        // ==========================================
        // 第二阶段：挂载基础运行组件
        // ==========================================
        bullet.AddComponent(new BulletTag());
        bullet.AddComponent(new FactionComponent(sourceFaction));
        bullet.AddComponent(new PositionComponent(position.x, position.y, 0));
        // 此时的 baseSpeed 已经是被修饰过的值了
        bullet.AddComponent(new VelocityComponent(direction.x * baseSpeed, direction.y * baseSpeed));
        bullet.AddComponent(new ViewComponent(bulletGo, prefab));
        bullet.AddComponent(new LifetimeComponent { Duration = recipe.LifeTime });
        bullet.AddComponent(new CollisionComponent(recipe.HitRadius));
        bullet.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Enemy", "Player"))); 
        bullet.AddComponent(new NeedsPhysicsBakingTag());
        bullet.AddComponent(new NeedsVisualBakingTag());

        // ==========================================
        // 第三阶段：根据天赋/升级表赋予对应的能力附魔
        // ==========================================
        if (modifiers != null)
        {
            // 3. 暴击 (对应表: AddCrit, CritEnhance)
            if (modifiers.GetLevel("AddCrit") > 0)
            {
                int critEnhance = modifiers.GetLevel("CritEnhance");
                // 基础10%暴击，每级增加5%；暴伤基础150%，每级加20%
                if (UnityEngine.Random.value <= (0.1f + critEnhance * 0.05f))
                {
                    finalDamage *= (1.5f + critEnhance * 0.2f);
                    bullet.AddComponent(new CriticalBulletComponent());
                }
            }

            // 4. 范围爆炸 (对应表: AddExplosion, ExplosionRadius)
            if (modifiers.GetLevel("AddExplosion") > 0)
            {
                int radiusLvl = modifiers.GetLevel("ExplosionRadius");
                // 基础半径2米，每级增加0.5米
                bullet.AddComponent(new AOEComponent(2f + radiusLvl * 0.5f));
            }

            // 5. 冰霜减速 (对应表: AddSlow, SlowEnhance)
            if (modifiers.GetLevel("AddSlow") > 0) 
            {
                int slowLvl = modifiers.GetLevel("SlowEnhance");
                // 基础减速50%，持续2秒；强化提升减速比例和持续时间
                bullet.AddComponent(new SlowEffectComponent(0.5f + slowLvl * 0.05f, 2f + slowLvl * 0.5f));
            }

            // 6. 连锁闪电 (对应表: AddLightning, LightningEnhance)
            if (modifiers.GetLevel("AddLightning") > 0) 
            {
                int lightLvl = modifiers.GetLevel("LightningEnhance");
                // 基础弹跳3次，范围5米；强化增加弹跳次数和寻敌距离
                bullet.AddComponent(new ChainComponent(3 + lightLvl, 5f + lightLvl * 0.5f));
            }

            // 7. 烈焰点燃 (对应表: AddBurn, BurnEnhance)
            if (modifiers.GetLevel("AddBurn") > 0)
            {
                int lvl = modifiers.GetLevel("BurnEnhance");
                bullet.AddComponent(new BulletDOTPayloadComponent(5f + lvl * 2f, 3f + lvl * 1f, "BurnVFX"));
            }

            // 8. 剧毒传染 (对应表: AddPoison, PoisonEnhance)
            if (modifiers.GetLevel("AddPoison") > 0)
            {
                int lvl = modifiers.GetLevel("PoisonEnhance");
                bullet.AddComponent(new BulletDOTPayloadComponent(3f + lvl * 1.5f, 5f + lvl * 2f, "PoisonVFX"));
            }

            // 【额外兼容】：保留穿透和眩晕，防止其他职业(如近战)或局外天赋系统用到这俩词条
            if (modifiers.GetLevel("AddPierce") > 0)
            {
                bullet.AddComponent(new PierceComponent(2 + modifiers.GetLevel("PierceEnhance")));
            }
            if (modifiers.GetLevel("AddStun") > 0) causeRecovery = true;
        }

        // ==========================================
        // 第四阶段：最终伤害与受击反馈结算
        // ==========================================
        bullet.AddComponent(new ImpactFeedbackComponent(bounce: false, recovery: causeRecovery, stunDurationOverride));
        bullet.AddComponent(new DamageComponent(finalDamage));

        return bullet;
    }
}