using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 特效实例化系统（表现层）
/// 修复：添加了渲染烘焙标签和 Z 轴偏移，确保特效可见。
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
        var events = GetEntitiesWith<VFXSpawnEventComponent>();

        for (int i = events.Count - 1; i >= 0; i--)
        {
            var evtEntity = events[i];
            var vfxEvent = evtEntity.GetComponent<VFXSpawnEventComponent>();

            if (_vfxStrategies.TryGetValue(vfxEvent.VFXType, out var setupAction))
            {
                setupAction.Invoke(vfxEvent);
            }

            // 事件消费完毕，立即标记销毁意图实体
            if (!evtEntity.HasComponent<PendingDestroyComponent>())
            {
                evtEntity.AddComponent(new PendingDestroyComponent());
            }
        }
        ReturnListToPool(events);
    }

    private void SetupSlowVFX(VFXSpawnEventComponent evt)
    {
        var target = evt.AttachTarget;
        var pool = GameObject_PoolManager.Instance;
        
        if (target != null && target.IsAlive && pool.SlowVFXPrefab != null)
        {
            var pos = target.GetComponent<PositionComponent>();
            Vector3 spawnPos = pos != null ? new Vector3(pos.X, pos.Y, -1f) : new Vector3(0, 0, -1f);
            
            GameObject go = pool.Spawn(pool.SlowVFXPrefab, spawnPos, Quaternion.identity);
            target.AddComponent(new AttachedVFXComponent(go));
        }
    }

    private void SetupExplosionVFX(VFXSpawnEventComponent evt)
    {
        var pool = GameObject_PoolManager.Instance;
        if (pool.ExplosionVFXPrefab != null)
        {
            GameObject go = pool.Spawn(pool.ExplosionVFXPrefab, evt.Position, Quaternion.identity);
            
            Entity vfxEntity = ECSManager.Instance.CreateEntity();
            // 设置 Z 为 -1 确保在最前方显示
            vfxEntity.AddComponent(new PositionComponent(evt.Position.x, evt.Position.y, -1f));
            vfxEntity.AddComponent(new ViewComponent(go, pool.ExplosionVFXPrefab));
            vfxEntity.AddComponent(new LifetimeComponent { Duration = 0.6f }); 
            // 必须添加此标签，否则渲染同步系统不会处理它
            vfxEntity.AddComponent(new NeedsVisualBakingTag()); 
        }
    }

    private void SetupLightningChainVFX(VFXSpawnEventComponent evt)
    {
        var pool = GameObject_PoolManager.Instance;
        if (pool.LightningChainVFX != null)
        {
            GameObject go = pool.Spawn(pool.LightningChainVFX, Vector3.zero, Quaternion.identity);
            
            Entity vfxEntity = ECSManager.Instance.CreateEntity();
            vfxEntity.AddComponent(new PositionComponent(evt.Position.x, evt.Position.y, -1f));
            vfxEntity.AddComponent(new ViewComponent(go, pool.LightningChainVFX));
            vfxEntity.AddComponent(new LightningVFXComponent(evt.Position, evt.EndPosition, 0.15f));
            vfxEntity.AddComponent(new NeedsVisualBakingTag()); 
        }
    }
}