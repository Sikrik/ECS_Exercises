using System.Collections.Generic;

/// <summary>
/// 实体回收系统：全场最后执行的系统。
/// 负责将所有被判死刑（PendingDestroyComponent）的实体，剥离视觉表现并安全丢入对象池。
/// </summary>
public class EntityCleanupSystem : SystemBase
{
    public EntityCleanupSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var deadEntities = GetEntitiesWith<PendingDestroyComponent>();

        // 倒序遍历，安全处理
        for (int i = deadEntities.Count - 1; i >= 0; i--)
        {
            var entity = deadEntities[i];

            // ==========================================
            // 1. 清理主模型 (ViewComponent)
            // ==========================================
            var view = entity.GetComponent<ViewComponent>();
            if (view != null && view.GameObject != null)
            {
                // 解除物理和逻辑的映射注册
                ECSManager.Instance.UnregisterView(view.GameObject);
                
                // 交给对象池回收，或直接销毁
                if (view.Prefab != null) 
                    GameObject_PoolManager.Instance.Despawn(view.Prefab, view.GameObject);
                else 
                    UnityEngine.Object.Destroy(view.GameObject);
            }

            // ==========================================
            // 2. 清理跟随特效 (AttachedVFXComponent)
            // ==========================================
            var vfx = entity.GetComponent<AttachedVFXComponent>();
            if (vfx != null && vfx.EffectObject != null)
            {
                UnityEngine.Object.Destroy(vfx.EffectObject);
            }

            // ==========================================
            // 3. 抹除逻辑存在 (彻底回收进池子)
            // ==========================================
            ECSManager.Instance.RemoveEntityInternal(entity);
        }

        ReturnListToPool(deadEntities);
    }
}