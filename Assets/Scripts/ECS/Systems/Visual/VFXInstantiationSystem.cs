using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 特效实例化系统（表现层）
/// 职责：监听逻辑层抛出的 VFX 事件，并从对象池中生成对应的视觉表现。
/// </summary>
public class VFXInstantiationSystem : SystemBase
{
    private readonly Dictionary<string, Action<VFXSpawnEventComponent>> _vfxStrategies;

    public VFXInstantiationSystem(List<Entity> entities) : base(entities) 
    {
        // 注册各种特效的生成策略
        _vfxStrategies = new Dictionary<string, Action<VFXSpawnEventComponent>>()
        {
            { "SlowVFX", SetupSlowVFX },
            { "Explosion", SetupExplosionVFX },
            { "LightningChain", SetupLightningChainVFX },
            { "MeleeSlash", SetupMeleeSlashVFX } // 【新增】近战挥砍特效策略
        };
    }

    public override void Update(float deltaTime)
    {
        // 1. 处理瞬时 VFX 事件
        var events = GetEntitiesWith<VFXSpawnEventComponent>();
        for (int i = events.Count - 1; i >= 0; i--)
        {
            var evtEntity = events[i];
            var vfxEvent = evtEntity.GetComponent<VFXSpawnEventComponent>();
            
            if (_vfxStrategies.TryGetValue(vfxEvent.VFXType, out var setupAction))
                setupAction.Invoke(vfxEvent);

            if (!evtEntity.HasComponent<PendingDestroyComponent>())
                evtEntity.AddComponent(new PendingDestroyComponent());
        }

        // 2. 处理蓄力/范围预测可视化 (保持原有的红框逻辑)
        UpdateDashPreviews();
    }

    // ==========================================
    // 【新增】：近战挥砍 VFX 生成策略
    // ==========================================
    private void SetupMeleeSlashVFX(VFXSpawnEventComponent evt)
    {
        // 假设你在 GameObject_PoolManager 中预留了一个叫 MeleeSlashPrefab 的槽位
        // 如果没有，你可以暂时借用 ExplosionVFXPrefab 或在 PoolManager 中新增引用
        GameObject prefab = GameObject_PoolManager.Instance.ExplosionVFXPrefab; 
        if (prefab == null) return;

        // 在逻辑位置生成特效
        GameObject go = GameObject_PoolManager.Instance.Spawn(prefab, evt.Position, Quaternion.identity);
        
        // 计算攻击方向以旋转特效
        Vector2 direction = (evt.EndPosition - evt.Position).normalized;
        if (direction.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            go.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // 自动管理生命周期：创建一个表现层实体来持有这个 GameObject
        Entity slashVfxEntity = ECSManager.Instance.CreateEntity();
        slashVfxEntity.AddComponent(new ViewComponent(go, prefab));
        
        // 赋予短暂寿命（如 0.2 秒），随后由 LifetimeSystem 自动回收
        slashVfxEntity.AddComponent(new LifetimeComponent { Duration = 0.2f });
    }

    // ==========================================
    // 现有策略 (保持不变)
    // ==========================================

    private void SetupLightningChainVFX(VFXSpawnEventComponent evt) 
    {
        GameObject prefab = GameObject_PoolManager.Instance.LightningChainVFX;
        if (prefab == null) return;

        GameObject go = GameObject_PoolManager.Instance.Spawn(prefab, evt.Position, Quaternion.identity);
        
        Entity lightningEntity = ECSManager.Instance.CreateEntity();
        lightningEntity.AddComponent(new ViewComponent(go, prefab));
        lightningEntity.AddComponent(new LightningVFXComponent(evt.Position, evt.EndPosition, 0.15f));
    }

    private void SetupExplosionVFX(VFXSpawnEventComponent evt) 
    { 
        GameObject prefab = GameObject_PoolManager.Instance.ExplosionVFXPrefab;
        if (prefab == null) return;

        GameObject go = GameObject_PoolManager.Instance.Spawn(prefab, evt.Position, Quaternion.identity);
        
        Entity expEntity = ECSManager.Instance.CreateEntity();
        expEntity.AddComponent(new ViewComponent(go, prefab));
        expEntity.AddComponent(new LifetimeComponent { Duration = 0.5f });
    }

    private void SetupSlowVFX(VFXSpawnEventComponent evt) 
    { 
        GameObject prefab = GameObject_PoolManager.Instance.SlowVFXPrefab;
        if (prefab == null || evt.AttachTarget == null) return;

        var view = evt.AttachTarget.GetComponent<ViewComponent>();
        if (view != null && view.GameObject != null)
        {
            GameObject go = GameObject_PoolManager.Instance.Spawn(prefab, view.GameObject.transform.position, Quaternion.identity);
            go.transform.SetParent(view.GameObject.transform);
            go.transform.localPosition = Vector3.zero;

            evt.AttachTarget.AddComponent(new AttachedVFXComponent(go));
        }
    }

    private void UpdateDashPreviews()
    {
        var previews = GetEntitiesWith<DashPreviewIntentComponent, ViewComponent>();
        foreach (var e in previews)
        {
            var previewIntent = e.GetComponent<DashPreviewIntentComponent>();
            var view = e.GetComponent<ViewComponent>();
            
            if (!e.HasComponent<ActiveDashPreviewComponent>())
            {
                GameObject prefab = GameObject_PoolManager.Instance.DashPreviewPrefab; 
                if (prefab == null) continue;

                GameObject go = GameObject_PoolManager.Instance.Spawn(prefab, view.GameObject.transform.position, Quaternion.identity);
                e.AddComponent(new ActiveDashPreviewComponent(go));
            }

            var activePreview = e.GetComponent<ActiveDashPreviewComponent>();
            if (activePreview.PreviewObject != null)
            {
                Transform t = activePreview.PreviewObject.transform;
                // 动态调整预警红框的缩放与位置
                float radius = e.HasComponent<CollisionComponent>() ? e.GetComponent<CollisionComponent>().Radius : 0.5f;
                float totalVisualLength = previewIntent.Distance + (radius * 2f);
                Vector3 dashDir = new Vector3(previewIntent.Direction.x, previewIntent.Direction.y, 0);

                t.localScale = new Vector3(totalVisualLength, previewIntent.Width, 1f);
                t.position = view.GameObject.transform.position + (dashDir * ((totalVisualLength * 0.5f) - radius));
                
                float angle = Mathf.Atan2(previewIntent.Direction.y, previewIntent.Direction.x) * Mathf.Rad2Deg;
                t.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        // 清理过期的预览
        var activePreviews = GetEntitiesWith<ActiveDashPreviewComponent>();
        for (int i = activePreviews.Count - 1; i >= 0; i--)
        {
            var e = activePreviews[i];
            if (!e.HasComponent<DashPreviewIntentComponent>())
            {
                var ap = e.GetComponent<ActiveDashPreviewComponent>();
                GameObject_PoolManager.Instance.Despawn(GameObject_PoolManager.Instance.DashPreviewPrefab, ap.PreviewObject);
                e.RemoveComponent<ActiveDashPreviewComponent>();
            }
        }
    }
}