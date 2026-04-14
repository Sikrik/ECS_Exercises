// 路径: Assets/Scripts/ECS/Core/ComponentTypeManager.cs
using System;
using System.Collections.Generic;

public static class ComponentTypeManager
{
    private static int _nextId = 0;
    private static readonly Dictionary<Type, int> _typeIds = new Dictionary<Type, int>();

    public static int GetId<T>() where T : Component
    {
        Type type = typeof(T);
        if (!_typeIds.TryGetValue(type, out int id))
        {
            id = _nextId++;
            if (id >= 128) throw new Exception("组件种类超过 128 种，请扩大 BitMask 容量！");
            _typeIds[type] = id;
        }
        return id;
    }
}