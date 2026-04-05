using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 减速效果系统，处理减速效果的计时与移除
/// </summary>
public class SlowEffectSystem : SystemBase
{
    public SlowEffectSystem(List<Entity> entities) : base(entities) { }
    
    public override void Update(float deltaTime)
    {
        // 筛选所有带有减速效果的实体
        var slowedEntities = GetEntitiesWith<SlowEffectComponent>();
        
        // 倒序遍历，避免删除元素导致索引错乱
        for (int i = slowedEntities.Count - 1; i >= 0; i--)
        {
            var entity = slowedEntities[i];
            var slowEffect = entity.GetComponent<SlowEffectComponent>();
            
            // 减少剩余时间
            slowEffect.RemainingDuration -= deltaTime;
            
            // 时间耗尽，移除减速组件
            if (slowEffect.RemainingDuration <= 0)
            {
                // 特效结束，销毁持续视觉特效
                if (slowEffect.EffectObject != null)
                {
                    Object.Destroy(slowEffect.EffectObject);
                }
                entity.RemoveComponent<SlowEffectComponent>();
            }
        }
    }
}