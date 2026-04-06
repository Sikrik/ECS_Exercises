using System.Collections.Generic;

/// <summary>
/// 事件清理系统：负责在每帧结束时移除所有瞬时的事件组件，防止逻辑重复触发。
/// </summary>
public class EventCleanupSystem : SystemBase
{
    public EventCleanupSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 1. 清理通用碰撞事件
        // 我们需要找到所有还挂载着 CollisionEventComponent 的实体
        var collisionEvents = GetEntitiesWith<CollisionEventComponent>();
        
        // 必须使用倒序遍历或临时记录，因为移除组件会改变查询结果缓存
        for (int i = collisionEvents.Count - 1; i >= 0; i--)
        {
            collisionEvents[i].RemoveComponent<CollisionEventComponent>();
        }

        // 2. 如果后续有其他瞬时事件（如 ExpCollectEventComponent），也在这里统一清理
        /*
        var expEvents = GetEntitiesWith<ExpCollectEventComponent>();
        for (int i = expEvents.Count - 1; i >= 0; i--)
        {
            expEvents[i].RemoveComponent<ExpCollectEventComponent>();
        }
        */
    }
}