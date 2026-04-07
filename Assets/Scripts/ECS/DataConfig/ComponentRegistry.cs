// Assets/Scripts/ECS/DataConfig/ComponentRegistry.cs

using System;
using System.Collections.Generic;

/// <summary>
/// 特性注册表：负责将 CSV 中的字符串标签（Trait）翻译为具体的逻辑组件（Component）。
/// 重构点：移除对 EnemyBuilderExtensions 的依赖，直接执行挂载，作为 Traits 的唯一出口。
/// </summary>
public static class ComponentRegistry
{
    // 字符串映射到“添加动作”
    private static readonly Dictionary<string, Action<Entity>> _map = new()
    {
        // 1. 物理特性
        { "Bouncy", e => e.AddComponent(new BouncyTag()) }, 
        
        // 2. 行为特性
        { "Ranged", e => e.AddComponent(new RangedTag()) }, 
        
        // 3. 未来扩展（例如爆炸属性）
        // { "Explosive", e => e.AddComponent(new ExplosiveComponent()) }
    };

    /// <summary>
    /// 根据名称为实体应用特定的特性。
    /// </summary>
    public static void Apply(Entity e, string traitName)
    {
        if (string.IsNullOrWhiteSpace(traitName)) return;

        if (_map.TryGetValue(traitName, out var action))
        {
            action(e);
        }
        else
        {
            // 如果 CSV 填错了，这里可以给个警告，方便排查填表错误
            UnityEngine.Debug.LogWarning($"[ComponentRegistry] 未定义的特性标签: {traitName}");
        }
    }
}