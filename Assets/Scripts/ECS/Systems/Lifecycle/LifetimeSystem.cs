using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用生命周期系统：负责销毁所有“限时存在”的实体（子弹、特效等）
/// </summary>
public class LifetimeSystem : SystemBase
{
    public LifetimeSystem(List<Entity> entities) : base(entities) { }
    
    public override void Update(float deltaTime)
    {
        // 筛选出所有拥有 LifetimeComponent 的实体
        var entities = GetEntitiesWith<LifetimeComponent>();
        
        // 使用倒序遍历，防止删除实体导致列表索引出错
        for (int i = entities.Count - 1; i >= 0; i--)
        {
            var entity = entities[i];
            var lifetime = entity.GetComponent<LifetimeComponent>();
            
            // 1. 扣除剩余时间
            lifetime.Duration -= deltaTime;
            
            // 2. 如果寿命结束，销毁实体
            if (lifetime.Duration <= 0)
            {
                // 调用 ECSManager 的统一销毁接口
                // 该接口会根据 ViewComponent 中的 Prefab 引用自动调用 PoolManager.Despawn
                entity.AddComponent(new PendingDestroyComponent());
            }
           
        }
    }
}