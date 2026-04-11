using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 特效实例化系统（表现层）
/// 职责：监听并消费特效生成事件 (VFXSpawnEventComponent)，从对象池中取出对应的特效预制体。
/// </summary>
public class VFXInstantiationSystem : SystemBase
{
    public VFXInstantiationSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 抓取本帧所有的特效生成意图
        var events = GetEntitiesWith<VFXSpawnEventComponent>();
        var ecs = ECSManager.Instance;
        var pool = GameObject_PoolManager.Instance;

        for (int i = events.Count - 1; i >= 0; i--)
        {
            var evtEntity = events[i];
            var vfxEvent = evtEntity.GetComponent<VFXSpawnEventComponent>();

            // 1. 处理减速持续性特效 (附着在目标身上)
            if (vfxEvent.VFXType == "SlowVFX" && vfxEvent.AttachTarget != null)
            {
                var target = vfxEvent.AttachTarget;
                if (target.IsAlive && pool.SlowVFXPrefab != null)
                {
                    // 获取目标当前位置
                    var pos = target.GetComponent<PositionComponent>();
                    Vector3 spawnPos = pos != null ? new Vector3(pos.X, pos.Y, 0) : Vector3.zero;
                    
                    // 从对象池生成特效对象
                    GameObject go = pool.Spawn(pool.SlowVFXPrefab, spawnPos, Quaternion.identity);
                    
                    // 将特效挂载到目标实体上，等待 SlowEffectSystem 在持续时间结束后销毁它
                    target.AddComponent(new AttachedVFXComponent(go));
                }
            }
            // 2. 处理爆炸瞬间特效
            else if (vfxEvent.VFXType == "Explosion" && pool.ExplosionVFXPrefab != null)
            {
                GameObject go = pool.Spawn(pool.ExplosionVFXPrefab, vfxEvent.Position, Quaternion.identity);
                
                // 为爆炸特效单独创建一个 ECS 实体来管理生命周期（假定爆炸动画持续 0.5 秒）
                Entity vfxEntity = ecs.CreateEntity();
                vfxEntity.AddComponent(new PositionComponent(vfxEvent.Position.x, vfxEvent.Position.y, 0));
                vfxEntity.AddComponent(new ViewComponent(go, pool.ExplosionVFXPrefab));
                vfxEntity.AddComponent(new LifetimeComponent { Duration = 0.5f }); 
            }
            // 3. 处理闪电链线段特效
            else if (vfxEvent.VFXType == "LightningChain" && pool.LightningChainVFX != null)
            {
                // 生成线段绘制器
                GameObject go = pool.Spawn(pool.LightningChainVFX, Vector3.zero, Quaternion.identity);
                
                // 创建 ECS 实体，交给 LightningRenderSystem 处理抖动渲染，并由它管理生命周期
                Entity vfxEntity = ecs.CreateEntity();
                vfxEntity.AddComponent(new ViewComponent(go, pool.LightningChainVFX));
                vfxEntity.AddComponent(new LightningVFXComponent(vfxEvent.Position, vfxEvent.EndPosition, 0.15f));
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
}