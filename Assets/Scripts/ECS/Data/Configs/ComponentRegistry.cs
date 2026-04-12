// 路径: Assets/Scripts/ECS/Data/Configs/ComponentRegistry.cs
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 特性注册表 (Data-Driven 装配中心)
/// </summary>
public static class ComponentRegistry
{
    private static readonly Dictionary<string, Action<Entity>> _map = new()
    {
        // 👇 【修复 5】增加 Bouncy 的空注册（因为反弹力度的读取已经放在了工厂里）
        { "Bouncy", e => { /* 仅作为标记，逻辑已在工厂处理 */ } },

        // ==========================================
        // 2. 行为与 AI 特性 (模块化组装)
        // ==========================================
        
        // 赋予怪物预判玩家走位的能力
        { "Predictive", e => e.AddComponent(new PredictiveAIComponent(0.6f)) },
        
        // 赋予怪物风筝能力和武器
        { "Ranged", e => 
            {
                e.AddComponent(new RangedTag());
                // 保持 7 米距离，距离容差 1 米
                e.AddComponent(new RangedAIComponent(7f, 1f)); 
                // 挂载武器组件：发射普通子弹，射击间隔 1.5 秒
                e.AddComponent(new WeaponComponent(BulletType.Normal, 1.5f)); 
            } 
        },
        
        // 赋予怪物绕开同类、形成包围圈的能力
        { "Swarm", e => e.AddComponent(new SwarmSeparationComponent(1.2f)) },
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
            Debug.LogWarning($"[ComponentRegistry] 装配失败：未定义的特性标签 '{traitName}'，请检查 CSV 配置文件拼写。");
        }
    }
}