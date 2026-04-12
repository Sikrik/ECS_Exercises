using System.Collections.Generic;

/// <summary>
/// 事件清理系统：负责在帧末统一销毁所有的瞬时事件组件。
/// 【终极优化版】：配合 EventPool 实现事件组件的循环利用，达成核心战斗 0 GC。
/// </summary>
public class EventCleanupSystem : SystemBase
{
    public EventCleanupSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // ==========================================
        // 1. 清理并回收【碰撞事件】
        // ==========================================
        var collisionEvents = GetEntitiesWith<CollisionEventComponent>();
        
        for (int i = collisionEvents.Count - 1; i >= 0; i--)
        {
            var entity = collisionEvents[i];
            var evt = entity.GetComponent<CollisionEventComponent>();
            
            // 从实体上移除组件（撕下标签）
            entity.RemoveComponent<CollisionEventComponent>(); 
            
            // 将组件对象还给对象池（洗干净放回池子待下次使用）
            EventPool.Return(evt); 
        }
        


        // ==========================================
        // 2. 清理并回收【受伤瞬时事件】
        // ==========================================
        var damageEvents = GetEntitiesWith<DamageTakenEventComponent>();
        
        for (int i = damageEvents.Count - 1; i >= 0; i--)
        {
            var entity = damageEvents[i];
            var evt = entity.GetComponent<DamageTakenEventComponent>();
            
            // 从实体上移除组件（撕下标签）
            entity.RemoveComponent<DamageTakenEventComponent>(); 
            
            // 将组件对象还给对象池（洗干净放回池子待下次使用）
            EventPool.Return(evt); 
        }
        
    }
}