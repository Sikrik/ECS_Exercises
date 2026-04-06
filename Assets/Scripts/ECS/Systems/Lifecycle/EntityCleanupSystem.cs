using System.Collections.Generic;

public class EntityCleanupSystem : SystemBase
{
    public EntityCleanupSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 筛选出本帧所有被标记为“待销毁”的实体
        var targets = GetEntitiesWith<DestroyTag>();

        // 倒序遍历，安全删除
        for (int i = targets.Count - 1; i >= 0; i--)
        {
            var entity = targets[i];
            
            // 1. 调用解耦后的回收器清理 Unity 表现层资源
            EntityRecycler.Cleanup(entity);
            
            // 2. 从 ECSManager 的核心列表中移除该实体
            ECSManager.Instance.RemoveEntityInternal(entity);
        }
    }
}