using System.Collections.Generic;
using UnityEngine;

public class VFXSystem : SystemBase
{
    public VFXSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 只有同时拥有位置和附加特效标记的实体才需要更新
        var entities = GetEntitiesWith<PositionComponent, AttachedVFXComponent>();

        foreach (var entity in entities)
        {
            var pos = entity.GetComponent<PositionComponent>();
            var vfx = entity.GetComponent<AttachedVFXComponent>();

            if (vfx.EffectObject != null)
            {
                // 将特效位置同步到实体的逻辑坐标
                vfx.EffectObject.transform.position = new Vector3(pos.X, pos.Y, 0);
            }
        }
    }
}