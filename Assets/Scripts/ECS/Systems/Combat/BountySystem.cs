using System.Collections.Generic;

public class BountySystem : SystemBase
{
    public BountySystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 只查询刚死掉、且带有悬赏金的实体
        var entities = GetEntitiesWith<DeadTag, BountyComponent>();
        
        for (int i = entities.Count - 1; i >= 0; i--)
        {
            var entity = entities[i];
            var bounty = entity.GetComponent<BountyComponent>();
            
            // 产生加分事件
            entity.AddComponent(new ScoreEventComponent(bounty.Score));
            
            // 剥夺悬赏组件，防止下一帧重复发钱
            entity.RemoveComponent<BountyComponent>();
        }
        ReturnListToPool(entities);
    }
}