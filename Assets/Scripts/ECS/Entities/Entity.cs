using System;
using System.Collections.Generic;

public class Entity
{
    // 正确写法：去掉 set 前面的 public 或 internal
    // 这样 get 和 set 都会默认跟随属性的 public 级别
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
}