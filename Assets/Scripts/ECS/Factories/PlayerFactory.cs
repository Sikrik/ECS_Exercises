using System;
using UnityEngine;

public static class PlayerFactory
{
    public static Entity Create(PlayerClass playerClass, GameObject prefab, GameConfig config)
    {
        var ecs = ECSManager.Instance;
        if (prefab == null) return null;

        string classId = playerClass.ToString();
        if (!config.PlayerRecipes.TryGetValue(classId, out var recipe))
        {
            Debug.LogError($"[PlayerFactory] 找不到职业配置：{classId}，请检查 Player_config.csv");
            return null;
        }

        // 1. 初始化基础属性
        float finalMaxHealth = recipe.MaxHealth;
        float finalMoveSpeed = recipe.MoveSpeed;
        float finalFireRate = recipe.FireRate;
        float finalAttack = 1.0f;      // 基础攻击倍率
        float expMultiplier = 1.0f;    // 基础经验倍率

        // 2. 【核心重构】：遍历局外天赋配置，动态应用所有加成
        if (GameDataManager.Instance != null && config.TalentRecipes != null)
        {
            foreach (var talentKvp in config.TalentRecipes)
            {
                string talentId = talentKvp.Key;
                TalentData data = talentKvp.Value;
                int level = GameDataManager.Instance.GetTalentLevel(talentId);

                if (level <= 0) continue;

                float totalBonus = level * data.ValuePerLevel;

                switch (data.TargetField)
                {
                    case "Health": finalMaxHealth += totalBonus; break;
                    case "Speed": finalMoveSpeed += totalBonus; break;
                    case "Exp": expMultiplier += totalBonus; break;
                    case "Attack": finalAttack += totalBonus; break; // 全局攻击倍率加成
                    // 攻速提升：数值是扣减射击间隔
                    case "FireRate": finalFireRate *= (1.0f - totalBonus); break; 
                }
            }
        }

        // 3. 实例化与组件组装
        GameObject go = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        Entity player = ecs.CreateEntity();
        
        // UI 与表现层
        var hudView = go.GetComponent<PlayerHUDView>();
        if (hudView != null && hudView.HealthRing != null && hudView.FlashIcon != null)
        {
            player.AddComponent(new PlayerHUDComponent(hudView.HealthRing, hudView.FlashIcon));
        }

        var indicatorView = go.GetComponent<DirectionIndicatorView>();
        if (indicatorView != null && indicatorView.ArrowPivot != null)
        {
            player.AddComponent(new DirectionIndicatorComponent(indicatorView.ArrowPivot, 6f, indicatorView.SyncPivots, 1.5f));
        }

        // 核心组件装配
        player.AddComponent(new PlayerTag());
        player.AddComponent(new PositionComponent(0, 0, 0));
        player.AddComponent(new ViewComponent(go, prefab));
        
        // 4. 注入计算后的最终属性 
        player.AddComponent(new HealthComponent(finalMaxHealth));
        player.AddComponent(new SpeedComponent(finalMoveSpeed)); 
        player.AddComponent(new VelocityComponent(0, 0)); 
        player.AddComponent(new MassComponent(recipe.Mass)); 
        player.AddComponent(new DashAbilityComponent(recipe.DashSpeed, recipe.DashDuration, recipe.DashCD));
        
        // 强制所有职业使用普通弹，不再读取配置，应用计算后的最终攻速
        player.AddComponent(new WeaponComponent(BulletType.Normal, finalFireRate));

        // 物理、机制与渲染基础组件
        player.AddComponent(new ImpactFeedbackComponent(bounce: true, recovery: false));
        player.AddComponent(new FactionComponent(FactionType.Player));
        player.AddComponent(new NeedsPhysicsBakingTag());
        player.AddComponent(new NeedsVisualBakingTag());
        player.AddComponent(new CollisionFilterComponent(0)); 
        player.AddComponent(new UIHealthUpdateEvent()); 
        
        // 注入动态计算出的经验倍率
        int initialExpReq = config.LevelExpRecipes.ContainsKey(1) ? config.LevelExpRecipes[1] : 50;
        player.AddComponent(new ExperienceComponent(initialExpReq, expMultiplier)); 
        
        // 注入修饰器，记录全局攻击力倍率供 BulletFactory 使用
        var modifiers = new WeaponModifierComponent();
        modifiers.GlobalDamageMultiplier = finalAttack; 
        player.AddComponent(modifiers); 

        return player;
    }
}