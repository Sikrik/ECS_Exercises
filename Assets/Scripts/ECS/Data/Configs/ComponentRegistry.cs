// 路径: Assets/Scripts/ECS/Data/Configs/ComponentRegistry.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public static class ComponentRegistry
{
    private static readonly Dictionary<string, Action<Entity>> _map = new()
    {
        { "Bouncy", e => { /* 标记处理 */ } },
        { "Predictive", e => e.AddComponent(new PredictiveAIComponent(0.6f)) },
        { "Ranged", e => {
            e.AddComponent(new RangedTag());
            e.AddComponent(new RangedAIComponent(7f, 1f, 8f, 1.0f)); 
            e.AddComponent(new WeaponComponent(BulletType.Normal, 1.5f)); 
        }},
        { "Swarm", e => e.AddComponent(new SwarmSeparationComponent(1.2f)) },

        // ==========================================
        // 【新增】近战职业初始化逻辑
        // ==========================================
        { "MeleeHero", e => {
            // 注入近战战斗核心组件
            e.AddComponent(new MeleeCombatComponent());
            // 近战职业的 WeaponComponent 仅用于控制挥砍节奏
            if (e.HasComponent<WeaponComponent>()) {
                e.GetComponent<WeaponComponent>().FireRate = 0.8f; 
            }
        }}
    };

    public static void Apply(Entity e, string traitName)
    {
        if (string.IsNullOrWhiteSpace(traitName)) return;
        if (_map.TryGetValue(traitName, out var action))
        {
            action(e);
        }
        else
        {
            Debug.LogWarning($"[ComponentRegistry] 未定义的特性标签 '{traitName}'");
        }
    }
}