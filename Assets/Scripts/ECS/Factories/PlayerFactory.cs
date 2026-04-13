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

        // ==========================================
        // 读取局外天赋数据，计算最终面板
        // ==========================================
        float finalMaxHealth = recipe.MaxHealth;
        float finalMoveSpeed = recipe.MoveSpeed;
        float expMultiplier = 1.0f; 

        if (GameDataManager.Instance != null)
        {
            int healthLevel = GameDataManager.Instance.GetTalentLevel("HealthUp");
            finalMaxHealth += healthLevel * 20f;

            int expLevel = GameDataManager.Instance.GetTalentLevel("ExpUp");
            expMultiplier += expLevel * 0.15f;
        }

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
        
        // 注入计算后的最终属性 
        player.AddComponent(new HealthComponent(finalMaxHealth));
        player.AddComponent(new SpeedComponent(finalMoveSpeed)); 
        player.AddComponent(new VelocityComponent(0, 0)); 
        player.AddComponent(new MassComponent(recipe.Mass)); 
        player.AddComponent(new DashAbilityComponent(recipe.DashSpeed, recipe.DashDuration, recipe.DashCD));
        
        // 【核心修改】：不再从表里读取子弹，统一初始化为普通弹
        player.AddComponent(new WeaponComponent(BulletType.Normal, recipe.FireRate));

        // 物理、机制与渲染基础组件
        player.AddComponent(new ImpactFeedbackComponent(bounce: true, recovery: false));
        player.AddComponent(new FactionComponent(FactionType.Player));
        player.AddComponent(new NeedsPhysicsBakingTag());
        player.AddComponent(new NeedsVisualBakingTag());
        player.AddComponent(new CollisionFilterComponent(0)); 
        player.AddComponent(new UIHealthUpdateEvent()); 
        
        // 【核心修改】：从配置表读取第1级的所需经验作为初始值，若无配置则兜底 50f
        int initialExpReq = config.LevelExpRecipes.ContainsKey(1) ? config.LevelExpRecipes[1] : 50;
        player.AddComponent(new ExperienceComponent(initialExpReq, expMultiplier)); 
        
        player.AddComponent(new WeaponModifierComponent()); 

        return player;
    }
}