using System.Collections.Generic;
using UnityEngine;

public class VFXCleanupSystem : SystemBase
{
    public VFXCleanupSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 表现层拦截逻辑层抛出的“意图”
        var entities = GetEntitiesWith<AttachedVFXComponent, PendingVFXDestroyTag>();
        
        foreach (var e in entities)
        {
            var vfx = e.GetComponent<AttachedVFXComponent>();
            if (vfx.EffectObject != null)
            {
                // 统一在这里管理 Unity 物体的生命周期
                Object.Destroy(vfx.EffectObject); 
            }
            
            e.RemoveComponent<AttachedVFXComponent>();
            e.RemoveComponent<PendingVFXDestroyTag>();
        }
    }
}