using System.Collections.Generic;

public class DeathCleanupSystem : SystemBase
{
    public DeathCleanupSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var deadEntities = GetEntitiesWith<DeadTag>();
        
        for (int i = deadEntities.Count - 1; i >= 0; i--)
        {
            var entity = deadEntities[i];
            
            // 正式下达物理销毁判决
            if (!entity.HasComponent<PendingDestroyComponent>())
            {
                entity.AddComponent(new PendingDestroyComponent());
            }
        }
        ReturnListToPool(deadEntities);
    }
}