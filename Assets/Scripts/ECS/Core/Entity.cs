// 路径: Assets/Scripts/ECS/Core/Entity.cs

using System;

public class Entity
{
    public bool IsAlive { get; set; } = true;
    
    // 替代 Dictionary，直接通过索引 O(1) 拿组件
    private Component[] _components = new Component[128];
    
    // 位掩码，用于极速判定是否拥有某些组件
    public ulong Mask0 { get; private set; } // 支持 ID 0-63
    public ulong Mask1 { get; private set; } // 支持 ID 64-127

    public void AddComponent<T>(T component) where T : Component
    {
        int id = ComponentTypeManager.GetId<T>();
        _components[id] = component;
        
        if (id < 64) Mask0 |= (1UL << id);
        else         Mask1 |= (1UL << (id - 64));
    }

    public T GetComponent<T>() where T : Component
    {
        return (T)_components[ComponentTypeManager.GetId<T>()];
    }

    public bool HasComponent<T>() where T : Component
    {
        int id = ComponentTypeManager.GetId<T>();
        if (id < 64) return (Mask0 & (1UL << id)) != 0;
        else         return (Mask1 & (1UL << (id - 64))) != 0;
    }

    public void RemoveComponent<T>() where T : Component
    {
        int id = ComponentTypeManager.GetId<T>();
        _components[id] = null;
        
        if (id < 64) Mask0 &= ~(1UL << id);
        else         Mask1 &= ~(1UL << (id - 64));
    }

    public void ClearComponents()
    {
        Array.Clear(_components, 0, _components.Length);
        Mask0 = 0;
        Mask1 = 0;
    }
}