// 路径: Assets/Scripts/ECS/Systems/Visual/VFXInstantiationSystem.cs
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
            { "MeleeSlash", SetupMeleeSlashVFX } // 近战挥砍特效策略
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

        // 2. 处理蓄力/范围预测可视化
        UpdateDashPreviews();
    }

    // ==========================================
    // 特效生成策略
    // ==========================================

    private void SetupMeleeSlashVFX(VFXSpawnEventComponent evt)
    {
        // 确保 GameObject_PoolManager 里有 MeleeSlashVFXPrefab
        GameObject prefab = GameObject_PoolManager.Instance.MeleeSlashVFXPrefab;
        if (prefab == null) return;

        GameObject go = GameObject_PoolManager.Instance.Spawn(prefab, evt.Position, Quaternion.identity);
        
        // 1. 获取半径和方向
        Vector2 direction = (evt.EndPosition - evt.Position).normalized;
        float radius = Vector2.Distance(evt.Position, evt.EndPosition);

        // 2. 旋转特效指向攻击方向
        if (direction.sqrMagnitude > 0.001f)
        {
            float lookAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            go.transform.rotation = Quaternion.Euler(0, 0, lookAngle);
        }

        // 3. 调用网格生成器
        var meshGenerator = go.GetComponent<MeleeSlashMeshGenerator>();
        if (meshGenerator != null)
        {
            float angle = evt.NumericParam > 0 ? evt.NumericParam : 90f;
            meshGenerator.GenerateSlashMesh(radius, angle, 20);
        }
        else
        {
            Debug.LogWarning("VFX Prefab 上没有挂载 MeleeSlashMeshGenerator 脚本！");
        }

        // 4. 自动管理生命周期
        Entity slashVfxEntity = ECSManager.Instance.CreateEntity();
        slashVfxEntity.AddComponent(new ViewComponent(go, prefab));
        slashVfxEntity.AddComponent(new LifetimeComponent { Duration = 0.2f }); // 刀光残留0.2秒
    }

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
                
                // 👇 使用 Length 而不是 Distance
                float totalVisualLength = previewIntent.Length + (radius * 2f);
                
                // 👇 使用 Unity 原生的 .x 和 .y
                Vector3 dashDir = new Vector3(previewIntent.Direction.x, previewIntent.Direction.y, 0);

                t.localScale = new Vector3(totalVisualLength, previewIntent.Width, 1f);
                t.position = view.GameObject.transform.position + (dashDir * ((totalVisualLength * 0.5f) - radius));
                
                // 👇 使用 Unity 原生的 .x 和 .y
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