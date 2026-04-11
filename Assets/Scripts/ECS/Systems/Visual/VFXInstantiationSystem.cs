using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 特效实例化系统（表现层 - OCP 高内聚重构版）
/// 职责：监听特效生成事件，从对象池中取出特效预制体并装配对应的 ECS 组件。
/// 优化：采用策略模式（查表法）消除 if-else，彻底符合开闭原则。
/// </summary>
public class VFXInstantiationSystem : SystemBase
{
    // 策略注册表：将特效的字符串 ID 映射到具体的生成装配方法
    private readonly Dictionary<string, Action<VFXSpawnEventComponent>> _vfxStrategies;

    public VFXInstantiationSystem(List<Entity> entities) : base(entities) 
    {
        // 【核心】：在这里注册所有的特效生成逻辑
        // 未来新增特效，只需在这里加一行映射，无需再修改 Update 循环
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

            // ==========================================
            // O(1) 查表执行，抛弃所有的 if-else
            // ==========================================
            if (_vfxStrategies.TryGetValue(vfxEvent.VFXType, out var setupAction))
            {
                setupAction.Invoke(vfxEvent);
            }
            else
            {
                // 防御性编程：捕获拼写错误或忘记注册的特效
                Debug.LogWarning($"<color=yellow>[VFXSystem]</color> 未知或未注册的特效类型: {vfxEvent.VFXType}");
            }

            // ==========================================
            // 事件消费完毕，给事件实体下达死亡判决
            // ==========================================
            if (!evtEntity.HasComponent<PendingDestroyComponent>())
            {
                evtEntity.AddComponent(new PendingDestroyComponent());
            }
        }

        // 归还查询列表，保持 0 GC
        ReturnListToPool(events);
    }

    // ==========================================
    // 原子化的装配策略 (分离不同特效的 ECS 构建逻辑)
    // ==========================================

    private void SetupSlowVFX(VFXSpawnEventComponent evt)
    {
        var target = evt.AttachTarget;
        var pool = GameObject_PoolManager.Instance;
        
        if (target != null && target.IsAlive && pool.SlowVFXPrefab != null)
        {
            var pos = target.GetComponent<PositionComponent>();
            Vector3 spawnPos = pos != null ? new Vector3(pos.X, pos.Y, 0) : Vector3.zero;
            
            GameObject go = pool.Spawn(pool.SlowVFXPrefab, spawnPos, Quaternion.identity);
            
            // 减速特效的特殊逻辑：挂载到目标实体上跟随
            target.AddComponent(new AttachedVFXComponent(go));
        }
    }

    private void SetupExplosionVFX(VFXSpawnEventComponent evt)
    {
        var pool = GameObject_PoolManager.Instance;
        
        if (pool.ExplosionVFXPrefab != null)
        {
            GameObject go = pool.Spawn(pool.ExplosionVFXPrefab, evt.Position, Quaternion.identity);
            
            // 爆炸特效的特殊逻辑：独立创建一个 Entity 仅用于管理生命周期（倒计时销毁）
            Entity vfxEntity = ECSManager.Instance.CreateEntity();
            vfxEntity.AddComponent(new PositionComponent(evt.Position.x, evt.Position.y, 0));
            vfxEntity.AddComponent(new ViewComponent(go, pool.ExplosionVFXPrefab));
            vfxEntity.AddComponent(new LifetimeComponent { Duration = 0.5f }); 
        }
    }

    private void SetupLightningChainVFX(VFXSpawnEventComponent evt)
    {
        var pool = GameObject_PoolManager.Instance;
        
        if (pool.LightningChainVFX != null)
        {
            GameObject go = pool.Spawn(pool.LightningChainVFX, Vector3.zero, Quaternion.identity);
            
            // 闪电链特效的特殊逻辑：需要挂载 LightningVFXComponent 支持两点之间的抖动渲染
            Entity vfxEntity = ECSManager.Instance.CreateEntity();
            vfxEntity.AddComponent(new ViewComponent(go, pool.LightningChainVFX));
            vfxEntity.AddComponent(new LightningVFXComponent(evt.Position, evt.EndPosition, 0.15f));
        }
    }
}