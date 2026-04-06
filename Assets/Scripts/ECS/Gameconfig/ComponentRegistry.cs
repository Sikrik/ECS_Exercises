using System;
using System.Collections.Generic;

public static class ComponentRegistry
{
    // 字符串映射到“添加动作”
    private static readonly Dictionary<string, Action<Entity>> _map = new()
    {
        { "Bouncy", e => e.WithBouncy() },
        { "Ranged", e => e.AddComponent(new RangedTag()) }, // 未来加远程只需在这里加一行
        // { "Explosive", e => e.AddComponent(new ExplosiveComponent()) }
    };

    public static void Apply(Entity e, string traitName)
    {
        if (_map.TryGetValue(traitName, out var action)) action(e);
    }
}