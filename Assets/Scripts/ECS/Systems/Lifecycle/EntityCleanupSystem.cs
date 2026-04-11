using System.Collections.Generic;

/// <summary>
/// 实体回收系统：全场最后执行的系统。
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
                ECSManager.Instance.UnregisterView(view.GameObject);
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
            // 3. 【新增】清理蓄力红框预览 (ActiveDashPreviewComponent)
            // ==========================================
            var activePreview = entity.GetComponent<ActiveDashPreviewComponent>();
            if (activePreview != null && activePreview.PreviewObject != null)
            {
                // 安全回收到对象池中，防止遗留在场景里
                GameObject_PoolManager.Instance.Despawn(
                    GameObject_PoolManager.Instance.DashPreviewPrefab, 
                    activePreview.PreviewObject
                );
            }

            // ==========================================
            // 4. 抹除逻辑存在 (彻底回收进池子)
            // ==========================================
            ECSManager.Instance.RemoveEntityInternal(entity);
        }

        ReturnListToPool(deadEntities);
    }
}