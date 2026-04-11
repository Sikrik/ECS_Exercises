using System.Collections.Generic;

public class HealthSystem : SystemBase
{
    public HealthSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var entities = GetEntitiesWith<HealthComponent>();
        
        for (int i = entities.Count - 1; i >= 0; i--)
        {
            var entity = entities[i];
            var health = entity.GetComponent<HealthComponent>();
            
            // 唯一职责：如果血量归零且还没死，宣告逻辑死亡
            if (health.CurrentHealth <= 0 && !entity.HasComponent<DeadTag>())
            {
                entity.AddComponent(new DeadTag());
            }
        }
        ReturnListToPool(entities);
    }
}