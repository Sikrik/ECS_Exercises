using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 生命值系统：负责监控所有生物的血量状态。
/// 重构后：通过 BountyComponent 实现通用的击杀奖励逻辑，不再依赖臃肿的 EnemyStats。
/// </summary>
public class HealthSystem : SystemBase
{
    public HealthSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 筛选拥有血量和表现层的所有实体
        var entities = GetEntitiesWith<HealthComponent, ViewComponent>();
        
        for (int i = entities.Count - 1; i >= 0; i--)
        {
            var entity = entities[i];
            var health = entity.GetComponent<HealthComponent>();
            
            // 判定死亡
            if (health.CurrentHealth <= 0)
            {
                // ==========================================
                // 1. 击杀奖励逻辑 (原子化解耦)
                // ==========================================
                // 只要实体挂载了“悬赏”组件，无论它是怪物、宝箱还是精英怪，都会产生加分事件
                if (entity.HasComponent<BountyComponent>())
                {
                    var bounty = entity.GetComponent<BountyComponent>();
                    entity.AddComponent(new ScoreEventComponent(bounty.Score));
                }
                
                // ==========================================
                // 2. 游戏状态判定
                // ==========================================
                if (entity.HasComponent<PlayerTag>())
                {
                    Debug.Log("游戏结束！");
                    Time.timeScale = 0; 
                    EventManager.Broadcast(new GameOverEvent());
                }

                // ==========================================
                // 3. 标记销毁
                // ==========================================
                // 统一贴上待销毁标签，由帧末的 EntityCleanupSystem 统一回收表现层和逻辑对象
                if (!entity.HasComponent<PendingDestroyComponent>())
                {
                    entity.AddComponent(new PendingDestroyComponent());
                }
            }
        }
        
        // 归还列表池，维持 0 GC
        ReturnListToPool(entities);
    }
}