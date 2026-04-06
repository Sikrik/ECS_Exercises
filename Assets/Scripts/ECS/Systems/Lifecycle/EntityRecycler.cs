using UnityEngine;

public static class EntityRecycler
{
    /// <summary>
    /// 专门负责清理实体关联的 Unity 资源和映射
    /// </summary>
    public static void Cleanup(Entity e)
    {
        var ecs = ECSManager.Instance;

        // 1. 处理视觉对象和映射回收
        if (e.HasComponent<ViewComponent>())
        {
            var view = e.GetComponent<ViewComponent>();
            if (view.GameObject != null)
            {
                // 从字典中注销映射
                var col = view.GameObject.GetComponentInChildren<Collider2D>();
                if (col != null) ecs.UnregisterView(col.gameObject);
                else ecs.UnregisterView(view.GameObject);

                // 回收到对象池
                if (view.Prefab != null)
                {
                    PoolManager.Instance.Despawn(view.Prefab, view.GameObject);
                }
                else
                {
                    Object.Destroy(view.GameObject);
                }
            }
        }

        // 2. 处理挂载的特效回收 (如减速烟雾)
        if (e.HasComponent<AttachedVFXComponent>())
        {
            var vfx = e.GetComponent<AttachedVFXComponent>();
            if (vfx.EffectObject != null)
            {
                PoolManager.Instance.Despawn(PoolManager.Instance.SlowVFXPrefab, vfx.EffectObject);
            }
        }

        // 3. 逻辑标记
        e.IsAlive = false;
    }
}