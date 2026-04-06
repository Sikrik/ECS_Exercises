using UnityEngine;

/// <summary>
/// 实体回收器：专门负责清理实体关联的 GameObject 和映射关系
/// </summary>
public static class EntityRecycler
{
    public static void Cleanup(Entity e)
    {
        var ecs = ECSManager.Instance;

        // 1. 处理 ViewComponent 关联的 GameObject 回收
        if (e.HasComponent<ViewComponent>())
        {
            var view = e.GetComponent<ViewComponent>();
            if (view.GameObject != null)
            {
                // 从映射表中注销，防止物理系统再找到已失效的物体
                ecs.UnregisterView(view.GameObject);

                // 回收到对象池或销毁
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

        // 2. 处理挂载的特效回收（如减速烟雾等）
        if (e.HasComponent<AttachedVFXComponent>())
        {
            var vfx = e.GetComponent<AttachedVFXComponent>();
            if (vfx.EffectObject != null)
            {
                // 这里假设减速特效使用的是 PoolManager 里的 SlowVFXPrefab
                PoolManager.Instance.Despawn(PoolManager.Instance.SlowVFXPrefab, vfx.EffectObject);
            }
        }

        // 3. 标记实体在逻辑上已死亡
        e.IsAlive = false;
    }
}