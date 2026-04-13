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
        player.AddComponent(new DashAbilityComponent(recipe.DashSpeed, recipe.DashDuration, recipe.DashCD));
        player.AddComponent(new WeaponComponent(BulletType.Normal, finalFireRate));
        player.AddComponent(new FactionComponent(FactionType.Player));
        player.AddComponent(new NeedsPhysicsBakingTag());
        player.AddComponent(new NeedsVisualBakingTag());

        // 3. 动态修饰器与经验
        var modifiers = new WeaponModifierComponent();
        modifiers.GlobalDamageMultiplier = finalAttack; 
        player.AddComponent(modifiers); 
        player.AddComponent(new ExperienceComponent(50f, 1.0f));

        // ==========================================
        // 【核心修改】动态职业特性注入
        // ==========================================
        // 如果是 Melee 职业，注入近战英雄特性
        if (playerClass == PlayerClass.Melee)
        {
            ComponentRegistry.Apply(player, "MeleeHero");
        }

        return player;
    }
}