using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 表现层特效清理系统
/// 职责：拦截逻辑层抛出的销毁意图 (PendingVFXDestroyTag)，
/// 安全地销毁 Unity 的 GameObject，维持逻辑层的纯净。
/// </summary>
public class VFXCleanupSystem : SystemBase
{
    public VFXCleanupSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 专门拦截那些带有特效组件，并且被逻辑层打上了“需要销毁特效”标签的实体
        var entities = GetEntitiesWith<AttachedVFXComponent, PendingVFXDestroyTag>();
        
        for (int i = entities.Count - 1; i >= 0; i--)
        {
            var e = entities[i];
            var vfx = e.GetComponent<AttachedVFXComponent>();
            
            if (vfx.EffectObject != null)
            {
                // 只有表现层的系统才有资格调用 Unity 的底层 API
                // 如果你以后为特效也做了对象池，把这行改成对应的 Despawn 即可
                Object.Destroy(vfx.EffectObject); 
            }
            
            // 剥离组件，完成特效的物理与逻辑清理
            e.RemoveComponent<AttachedVFXComponent>();
            e.RemoveComponent<PendingVFXDestroyTag>();
        }
        
        // 归还查询列表，保持 0 GC
        ReturnListToPool(entities);
    }
}