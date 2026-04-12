// Assets/Scripts/ECS/DataConfig/ComponentRegistry.cs

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 特性注册表 (Data-Driven 装配中心)
/// 职责：将 CSV 配置文件中的字符串标签（Trait）翻译为具体的逻辑组件（Component）。
/// 优势：作为 Traits 的唯一装配出口，彻底解耦数据解析与实体构建，方便扩展各种组合能力。
/// </summary>
public static class ComponentRegistry
{
    // 采用字典映射字符串到 Action 委托，避免冗长的 switch/case，提升查找速度与代码整洁度
    private static readonly Dictionary<string, Action<Entity>> _map = new()
    {
        // ==========================================
        // 1. 物理与受击特性
        // ==========================================
        
        // ==========================================
        // 2. 行为与 AI 特性 (模块化组装)
        // ==========================================
        
        // 赋予怪物预判玩家走位的能力（适合高移速的 Fast 怪物）
        { "Predictive", e => e.AddComponent(new PredictiveAIComponent(0.6f)) },
        
        // 赋予怪物风筝能力和武器（将其变成远程射手）
        { "Ranged", e => 
            {
                e.AddComponent(new RangedTag());
                // 保持 7 米距离，距离容差 1 米
                e.AddComponent(new RangedAIComponent(7f, 1f)); 
                // 挂载武器组件：发射普通子弹，射击间隔 1.5 秒
                e.AddComponent(new WeaponComponent(BulletType.Normal, 1.5f)); 
            } 
        },
        
        // 赋予怪物绕开同类、形成包围圈的能力（适合数量庞大的基础群怪）
        { "Swarm", e => e.AddComponent(new SwarmSeparationComponent(1.2f)) },

        // ==========================================
        // 3. 未来预留扩展接口
        // ==========================================
        // 例如：死亡自爆能力
        // { "Explosive", e => e.AddComponent(new ExplosiveComponent(radius: 3f, damage: 20f)) }
    };

    /// <summary>
    /// 核心装配方法：根据传入的特性名称，为目标实体动态挂载组件组合。
    /// </summary>
    /// <param name="e">目标实体</param>
    /// <param name="traitName">CSV 中配置的特性字符串</param>
    public static void Apply(Entity e, string traitName)
    {
        // 防御性校验，过滤空字符串
        if (string.IsNullOrWhiteSpace(traitName)) return;

        // 尝试从注册表中获取对应的装配动作并执行
        if (_map.TryGetValue(traitName, out var action))
        {
            action(e);
        }
        else
        {
            // 友好的容错提示：如果策划在 CSV 表格里拼写错了标签，控制台会立刻暴露问题
            Debug.LogWarning($"[ComponentRegistry] 装配失败：未定义的特性标签 '{traitName}'，请检查 CSV 配置文件拼写。");
        }
    }
}