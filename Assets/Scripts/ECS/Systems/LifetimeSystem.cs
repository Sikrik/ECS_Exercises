using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用生命周期系统：负责销毁所有“限时存在”的实体
/// </summary>
public class LifetimeSystem : SystemBase
{
    public LifetimeSystem(List<Entity> entities) : base(entities) { }
    
    public override void Update(float deltaTime)
    {
        // 凡是有倒计时的实体都要处理
        var entities = GetEntitiesWith<LifetimeComponent>();
        
        for (int i = entities.Count - 1; i >= 0; i--)
        {
            var entity = entities[i];
            var lifetime = entity.GetComponent<LifetimeComponent>();
            
            lifetime.RemainingTime -= deltaTime;
            
            if (lifetime.RemainingTime <= 0)
            {
                // 统一交由 Manager 销毁
                ECSManager.Instance.DestroyEntity(entity);
            }
        }
    }
}