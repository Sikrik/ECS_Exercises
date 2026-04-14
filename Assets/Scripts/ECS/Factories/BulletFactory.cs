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

        bullet.AddComponent(new BulletTag());
        bullet.AddComponent(new FactionComponent(sourceFaction));
        bullet.AddComponent(new PositionComponent(position.x, position.y, 0));
        bullet.AddComponent(new VelocityComponent(direction.x * baseSpeed, direction.y * baseSpeed));
        bullet.AddComponent(new ViewComponent(bulletGo, prefab));
        bullet.AddComponent(new LifetimeComponent { Duration = recipe.LifeTime });
        bullet.AddComponent(new CollisionComponent(recipe.HitRadius));
        bullet.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Enemy", "Player"))); 
        bullet.AddComponent(new NeedsPhysicsBakingTag());
        bullet.AddComponent(new NeedsVisualBakingTag());

        float finalDamage = baseDamage;
        bool causeRecovery = false;       
        float stunDurationOverride = 0f;  

        if (modifiers != null)
        {
            finalDamage *= modifiers.GlobalDamageMultiplier;
            finalDamage *= (1f + modifiers.GetLevel("AttackUp") * 0.2f);

            // 暴击
            if (modifiers.GetLevel("AddCrit") > 0)
            {
                int critEnhance = modifiers.GetLevel("CritEnhance");
                if (UnityEngine.Random.value <= (0.1f + critEnhance * 0.05f))
                {
                    finalDamage *= (1.5f + critEnhance * 0.2f);
                    bullet.AddComponent(new CriticalBulletComponent());
                }
            }

            // 穿透 (PierceComponent 已存在于 BulletAbilityComponents.cs)
            if (modifiers.GetLevel("AddPierce") > 0)
            {
                // 【修复3：修正穿透次数的数学逻辑】
                // 基础的 "AddPierce" 代表子弹能多穿透1个敌人（总共命中2个），所以基数应为 2
                bullet.AddComponent(new PierceComponent(2 + modifiers.GetLevel("PierceEnhance")));
            }

            // 燃烧 (火)
            if (modifiers.GetLevel("AddBurn") > 0)
            {
                int lvl = modifiers.GetLevel("BurnEnhance");
                bullet.AddComponent(new BulletDOTPayloadComponent(5f + lvl * 2f, 3f + lvl * 1f, "BurnVFX"));
            }

            // 中毒 (木)
            if (modifiers.GetLevel("AddPoison") > 0)
            {
                int lvl = modifiers.GetLevel("PoisonEnhance");
                bullet.AddComponent(new BulletDOTPayloadComponent(3f + lvl * 1.5f, 5f + lvl * 2f, "PoisonVFX"));
            }

            // 减速、闪电、AOE 等原有逻辑保持...
            if (modifiers.GetLevel("AddSlow") > 0) bullet.AddComponent(new SlowEffectComponent(0.5f, 2f));
            if (modifiers.GetLevel("AddChain") > 0) bullet.AddComponent(new ChainComponent(3, 5f));
            if (modifiers.GetLevel("AddAOE") > 0) bullet.AddComponent(new AOEComponent(2f));
            if (modifiers.GetLevel("AddStun") > 0) causeRecovery = true;
        }

        bullet.AddComponent(new ImpactFeedbackComponent(bounce: false, recovery: causeRecovery, stunDurationOverride));
        bullet.AddComponent(new DamageComponent(finalDamage));

        return bullet;
    }
}