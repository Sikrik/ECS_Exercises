using System;
using UnityEngine;

public static class PlayerFactory
{
    public static Entity Create(PlayerClass playerClass, GameObject prefab, GameConfig config)
    {
        var ecs = ECSManager.Instance;
        if (prefab == null) return null;

        // 1. 从字典中抓取对应的职业配方
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
            // 血量天赋：每级增加 20 点最大生命值
            int healthLevel = GameDataManager.Instance.GetTalentLevel("HealthUp");
            finalMaxHealth += healthLevel * 20f;

            // 经验天赋：每级增加 15% 的经验获取率
            int expLevel = GameDataManager.Instance.GetTalentLevel("ExpUp");
            expMultiplier += expLevel * 0.15f;
            
            // 如果你以后加了速度天赋 (SpeedUp)，也可以直接在这里写：
            // int speedLevel = GameDataManager.Instance.GetTalentLevel("SpeedUp");
            // finalMoveSpeed *= (1f + speedLevel * 0.05f);
        }

        // 2. 表现层实例化
        GameObject go = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);

        // 3. 逻辑层创建并组装
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
            // 【核心修改点】：将 indicatorView.SyncPivots 数组传递给组件，并设置 1.5f 的浮游延迟转速
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
        
        BulletType defaultBullet = Enum.TryParse<BulletType>(recipe.DefaultBullet, out var bType) ? bType : BulletType.Normal;
        player.AddComponent(new WeaponComponent(defaultBullet, recipe.FireRate));

        // 物理、机制与渲染基础组件
        player.AddComponent(new ImpactFeedbackComponent(bounce: true, recovery: false));
        player.AddComponent(new FactionComponent(FactionType.Player));
        player.AddComponent(new NeedsPhysicsBakingTag());
        player.AddComponent(new NeedsVisualBakingTag());
        player.AddComponent(new CollisionFilterComponent(0)); 
        player.AddComponent(new UIHealthUpdateEvent()); 
        
        // 注入经验倍率
        player.AddComponent(new ExperienceComponent(50f, expMultiplier)); 
        player.AddComponent(new WeaponModifierComponent()); 

        return player;
    }
}