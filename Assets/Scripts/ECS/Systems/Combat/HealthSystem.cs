using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 生命值系统（纯逻辑层）
/// 职责：监控血量，判定死亡，派发奖励和游戏状态事件
/// </summary>
public class HealthSystem : SystemBase
{
    public HealthSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 不再依赖 ViewComponent
        var entities = GetEntitiesWith<HealthComponent>();
        
        for (int i = entities.Count - 1; i >= 0; i--)
        {
            var entity = entities[i];
            var health = entity.GetComponent<HealthComponent>();
            
            // 判定死亡
            if (health.CurrentHealth <= 0)
            {
                // 1. 击杀奖励逻辑 (挂载单帧加分组件)
                if (entity.HasComponent<BountyComponent>())
                {
                    var bounty = entity.GetComponent<BountyComponent>();
                    entity.AddComponent(new ScoreEventComponent(bounty.Score));
                }
                
                // 2. 玩家死亡：触发游戏结束
                if (entity.HasComponent<PlayerTag>())
                {
                    Debug.Log("玩家死亡，抛出全局结束事件！");
                    Time.timeScale = 0; 
                    
                    // 【核心修改】：创建一个携带 GameOverEventComponent 的空白实体作为单帧事件
                    var eventEntity = ECSManager.Instance.CreateEntity();
                    eventEntity.AddComponent(new GameOverEventComponent());
                }

                // 3. 统一标记销毁
                if (!entity.HasComponent<PendingDestroyComponent>())
                {
                    entity.AddComponent(new PendingDestroyComponent());
                }
            }
        }
    }
}