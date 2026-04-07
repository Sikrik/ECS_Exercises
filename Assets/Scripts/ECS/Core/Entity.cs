using System;
using System.Collections.Generic;

public class Entity
{
    public bool IsAlive { get; set; } = true;
    
    private Dictionary<Type, Component> _components = new Dictionary<Type, Component>();

    public void AddComponent(Component component)
    {
        _components[component.GetType()] = component;
    }

    public T GetComponent<T>() where T : Component
    {
        if (_components.TryGetValue(typeof(T), out var component))
        {
            return (T)component;
        }
        return null;
    }

    public bool HasComponent<T>() where T : Component
    {
        return _components.ContainsKey(typeof(T));
    }

    public void RemoveComponent<T>() where T : Component
    {
        _components.Remove(typeof(T));
    }

    // 👇 就是缺了下面这个方法！它是用来给对象池回收时一键清空组件的
    public void ClearComponents()
    {
        _components.Clear();
    }
}