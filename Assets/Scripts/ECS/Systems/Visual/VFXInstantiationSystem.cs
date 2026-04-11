using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 特效实例化系统（表现层）
/// 职责：处理瞬时视觉事件，并管理持续性视觉反馈（如蓄力预测框）
/// </summary>
public class VFXInstantiationSystem : SystemBase
{
    private readonly Dictionary<string, Action<VFXSpawnEventComponent>> _vfxStrategies;

    public VFXInstantiationSystem(List<Entity> entities) : base(entities) 
    {
        _vfxStrategies = new Dictionary<string, Action<VFXSpawnEventComponent>>()
        {
            { "SlowVFX", SetupSlowVFX },
            { "Explosion", SetupExplosionVFX },
            { "LightningChain", SetupLightningChainVFX }
        };
    }

    public override void Update(float deltaTime)
    {
        // ==========================================
        // 1. 处理瞬时 VFX 事件
        // ==========================================
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
        ReturnListToPool(events);

        // ==========================================
        // 2. 处理蓄力范围预测可视化
        // ==========================================
        var previews = GetEntitiesWith<DashPreviewIntentComponent, ViewComponent>();
        foreach (var e in previews)
        {
            var previewIntent = e.GetComponent<DashPreviewIntentComponent>();
            var view = e.GetComponent<ViewComponent>();
            
            // 如果该实体还没有关联预览物体，则从池中获取
            if (!e.HasComponent<ActiveDashPreviewComponent>())
            {
                // 假设 GameObject_PoolManager 中有名为 DashPreviewPrefab 的槽位
                GameObject prefab = GameObject_PoolManager.Instance.DashPreviewPrefab; 
                if (prefab == null) continue;

                GameObject go = GameObject_PoolManager.Instance.Spawn(prefab, view.GameObject.transform.position, Quaternion.identity);
                e.AddComponent(new ActiveDashPreviewComponent(go));
            }

            // 更新预览物体的坐标、旋转和缩放
            var activePreview = e.GetComponent<ActiveDashPreviewComponent>();
            if (activePreview.PreviewObject != null)
            {
                Transform t = activePreview.PreviewObject.transform;
                
                // 【修复3】：修正 Pivot 偏移。将方块沿着冲刺方向往前推自身长度的一半，使其从怪物身前开始延伸
                Vector3 basePosition = view.GameObject.transform.position;
                Vector3 offset = new Vector3(previewIntent.Direction.x, previewIntent.Direction.y, 0) * (previewIntent.Distance * 0.5f);
                
                t.position = basePosition + offset;
                
                // 旋转指向冲刺方向
                float angle = Mathf.Atan2(previewIntent.Direction.y, previewIntent.Direction.x) * Mathf.Rad2Deg;
                t.rotation = Quaternion.Euler(0, 0, angle);
                
                // 缩放对应预测的距离和宽度 (假设 Prefab 原始长宽为 1x1)
                t.localScale = new Vector3(previewIntent.Distance, previewIntent.Width, 1f);
            }
        }
        ReturnListToPool(previews);
        
        // ==========================================
        // 3. 清理已结束蓄力的预览物体
        // ==========================================
        var activePreviews = GetEntitiesWith<ActiveDashPreviewComponent>();
        for (int i = activePreviews.Count - 1; i >= 0; i--)
        {
            var e = activePreviews[i];
            // 如果实体已经没有预测意图了（蓄力结束），回收预览物体
            if (!e.HasComponent<DashPreviewIntentComponent>())
            {
                var ap = e.GetComponent<ActiveDashPreviewComponent>();
                GameObject_PoolManager.Instance.Despawn(GameObject_PoolManager.Instance.DashPreviewPrefab, ap.PreviewObject);
                e.RemoveComponent<ActiveDashPreviewComponent>();
            }
        }
        ReturnListToPool(activePreviews);
    }

    // --- 原有 Setup 方法保持不变，留空或保留你的实现 ---
    private void SetupSlowVFX(VFXSpawnEventComponent evt) { /* 视你的具体实现而定 */ }
    private void SetupExplosionVFX(VFXSpawnEventComponent evt) { /* 视你的具体实现而定 */ }
    private void SetupLightningChainVFX(VFXSpawnEventComponent evt) { /* 视你的具体实现而定 */ }
}