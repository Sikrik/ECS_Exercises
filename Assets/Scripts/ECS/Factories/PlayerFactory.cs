// 路径: Assets/Scripts/ECS/Factories/PlayerFactory.cs
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

        // 2. 表现层实例化
        GameObject go = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);

        // 3. 逻辑层创建并组装
        Entity player = ecs.CreateEntity();
        
        // ==========================================
        // 挂载 PlayerHUD 和 方向指示器 (纯 UI 与表现层)
        // ==========================================
        var hudView = go.GetComponent<PlayerHUDView>();
        if (hudView != null && hudView.HealthRing != null && hudView.FlashIcon != null)
        {
            player.AddComponent(new PlayerHUDComponent(hudView.HealthRing, hudView.FlashIcon));
        }

        var indicatorView = go.GetComponent<DirectionIndicatorView>();
        if (indicatorView != null && indicatorView.ArrowPivot != null)
        {
            player.AddComponent(new DirectionIndicatorComponent(indicatorView.ArrowPivot, 6f));
        }

        // ==========================================
        // 核心组件装配 (彻底由 CSV 数据驱动)
        // ==========================================
        player.AddComponent(new PlayerTag());
        player.AddComponent(new PositionComponent(0, 0, 0));
        player.AddComponent(new ViewComponent(go, prefab));
        
        // 动态数值：生命、速度、质量、无敌时间
        player.AddComponent(new HealthComponent(recipe.MaxHealth));
        player.AddComponent(new SpeedComponent(recipe.MoveSpeed)); 
        player.AddComponent(new VelocityComponent(0, 0)); 
        player.AddComponent(new MassComponent(recipe.Mass)); 
        player.AddComponent(new DashAbilityComponent(recipe.DashSpeed, recipe.DashDuration, recipe.DashCD));
        
        // 解析默认武器
        BulletType defaultBullet = Enum.TryParse<BulletType>(recipe.DefaultBullet, out var bType) ? bType : BulletType.Normal;
        player.AddComponent(new WeaponComponent(defaultBullet, recipe.FireRate));

        // ==========================================
        // 物理、机制与渲染基础组件
        // ==========================================
        player.AddComponent(new BouncyTag());
        player.AddComponent(new ImpactFeedbackComponent(bounce: true, recovery: false));
        player.AddComponent(new FactionComponent(FactionType.Player));
        player.AddComponent(new NeedsPhysicsBakingTag());
        player.AddComponent(new NeedsVisualBakingTag());
        player.AddComponent(new CollisionFilterComponent(0)); // 0 代表与任何能撞的层级都交互
        player.AddComponent(new UIHealthUpdateEvent()); 

        return player;
    }
}