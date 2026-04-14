// 路径: Assets/Scripts/ECS/Factories/PlayerFactory.cs
using System;
using UnityEngine;

public static class PlayerFactory
{
    public static Entity Create(PlayerClass playerClass, GameObject prefab, GameConfig config)
    {
        var ecs = ECSManager.Instance;
        if (prefab == null) return null;

        string classId = playerClass.ToString();
        if (!config.PlayerRecipes.TryGetValue(classId, out var recipe)) return null;

        // 1. 基础属性初始化
        float finalMaxHealth = recipe.MaxHealth;
        float finalMoveSpeed = recipe.MoveSpeed;
        float finalFireRate = recipe.FireRate;
        float finalAttack = 1.0f;

        // 2. 实例化与组件组装
        GameObject go = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        Entity player = ecs.CreateEntity();
        
        player.AddComponent(new PlayerTag());
        player.AddComponent(new PositionComponent(0, 0, 0));
        player.AddComponent(new ViewComponent(go, prefab));
        player.AddComponent(new HealthComponent(finalMaxHealth));
        player.AddComponent(new SpeedComponent(finalMoveSpeed)); 
        player.AddComponent(new VelocityComponent(0, 0)); 
        player.AddComponent(new MassComponent(recipe.Mass)); 
        // 【新增】给玩家添加碰撞过滤层，使其能够主动检测与敌人的碰撞
        player.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Enemy", "Player")));
        player.AddComponent(new DashAbilityComponent(recipe.DashSpeed, recipe.DashDuration, recipe.DashCD));
        player.AddComponent(new WeaponComponent(BulletType.Normal, finalFireRate));
        player.AddComponent(new FactionComponent(FactionType.Player));
        player.AddComponent(new NeedsPhysicsBakingTag());
        player.AddComponent(new NeedsVisualBakingTag());

        // ==========================================
        // 【核心挂载】：挂载方向指示器与随身 HUD 组件
        // 将纯视觉的旋转和 UI 刷新交还给 DirectionIndicatorSystem 和 UISyncSystem
        // ==========================================
        var indicatorView = go.GetComponent<DirectionIndicatorView>();
        if (indicatorView != null && indicatorView.ArrowPivot != null)
        {
            // 参数: 主箭头, 主箭头旋转速度, 额外跟随物数组(血环/闪电等), 额外跟随物旋转速度
            player.AddComponent(new DirectionIndicatorComponent(indicatorView.ArrowPivot, 3f, indicatorView.SyncPivots, 2f));
        }

        var hudView = go.GetComponent<PlayerHUDView>();
        if (hudView != null)
        {
            player.AddComponent(new PlayerHUDComponent(hudView.HealthRing, hudView.FlashIcon));
        }

        // 3. 动态修饰器与经验
        var modifiers = new WeaponModifierComponent();
        modifiers.GlobalDamageMultiplier = finalAttack; 
        player.AddComponent(modifiers); 
        player.AddComponent(new ExperienceComponent(50f, 1.0f));

        // ==========================================
        // 【特性注入】：动态职业特性注入
        // ==========================================
        if (playerClass == PlayerClass.Melee)
        {
            ComponentRegistry.Apply(player, "MeleeHero");
        }

        return player;
    }
}