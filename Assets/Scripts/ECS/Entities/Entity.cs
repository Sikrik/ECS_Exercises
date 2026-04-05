using System.Collections.Generic;
/// <summary>
/// ECS架构中的实体类，作为组件的容器，本身不包含业务逻辑
/// 实体仅用于标识一个游戏对象，所有状态数据都存储在其绑定的组件中
/// </summary>
public class Entity
{
    /// <summary>
    /// 静态计数器，用于为每个新实体分配全局唯一的ID
    /// </summary>
    private static int _nextId = 0;
    
    /// <summary>
    /// 当前实体的唯一标识ID
    /// </summary>
    public int Id { get; private set; }
    
    /// <summary>
    /// 当前实体是否存活（用于标记删除，避免List的O(n)删除开销）
    /// </summary>
    public bool IsAlive { get; private set; }
    
    /// <summary>
    /// 当前实体绑定的所有组件，键为组件的类型，值为对应的组件实例
    /// </summary>
    private Dictionary<System.Type,Component> _components;
    
    /// <summary>
    /// 初始化一个新的实体实例
    /// </summary>
    public Entity()
    {
        Id = _nextId++;
        _components = new Dictionary<System.Type, Component>();
        IsAlive = true;
    }
    
    /// <summary>
    /// 标记实体为已死亡，用于标记删除
    /// </summary>
    public void MarkAsDead()
    {
        IsAlive = false;
    }
    
    /// <summary>
    /// 为当前实体添加一个组件
    /// </summary>
    /// <param name="component">要添加的组件实例</param>
    public void AddComponent(Component component)
    {
        var type = component.GetType();
        if (_components.ContainsKey(type))
        {
            // 重复添加时覆盖，用于刷新效果（如减速时间）
            _components[type] = component;
            return;
        }
        _components.Add(type, component);
    }
    
    /// <summary>
    /// 获取当前实体的指定类型组件
    /// </summary>
    /// <typeparam name="T">要获取的组件类型</typeparam>
    /// <returns>如果存在对应组件则返回实例，否则返回null</returns>
    public T GetComponent<T>()where T : Component
    {
        if (_components.TryGetValue(typeof(T), out Component comp))
        {
            return (T)comp;
        }
        return null;
    }
    
    /// <summary>
    /// 移除当前实体的指定类型组件
    /// </summary>
    /// <typeparam name="T">要移除的组件类型</typeparam>
    public void RemoveComponent<T>() where T : Component
    {
        _components.Remove(typeof(T));
    }
    
    /// <summary>
    /// 检查当前实体是否拥有指定类型的组件
    /// </summary>
    /// <typeparam name="T">要检查的组件类型</typeparam>
    /// <returns>如果拥有该组件返回true，否则返回false</returns>
    public bool HasComponent<T>() where T : Component
    {
        return _components.ContainsKey(typeof(T));
    }
    
}