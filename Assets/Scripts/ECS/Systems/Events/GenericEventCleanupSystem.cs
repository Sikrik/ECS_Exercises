using System.Collections.Generic;

/// <summary>
/// 通用事件清理系统 (终极解耦版)
/// 任何事件只要注册了这个系统，就会在帧末被自动回收
/// </summary>
public class GenericEventCleanupSystem<T> : SystemBase where T : Component, IPooledEvent, new()
{
    public GenericEventCleanupSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var events = GetEntitiesWith<T>();
        
        // 倒序遍历防止移除组件时破坏索引
        for (int i = events.Count - 1; i >= 0; i--)
        {
            var entity = events[i];
            var evt = entity.GetComponent<T>();
            
            // 1. 从实体上撕下标签
            entity.RemoveComponent<T>(); 
            
            // 2. 扔回该类型的专属泛型对象池
            EventPool<T>.Return(evt); 
        }
    }
}