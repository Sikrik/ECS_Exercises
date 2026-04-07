using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : SystemBase
{
    public HealthSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var entities = GetEntitiesWith<HealthComponent, ViewComponent>();
        
        for (int i = entities.Count - 1; i >= 0; i--)
        {
            var entity = entities[i];
            var health = entity.GetComponent<HealthComponent>();
            
            if (health.CurrentHealth <= 0)
            {
                // 【修改点】抛出加分事件，而不是直接改分数
                if (entity.HasComponent<EnemyTag>() && entity.HasComponent<EnemyStatsComponent>())
                {
                    var stats = entity.GetComponent<EnemyStatsComponent>();
                    
                    // 创建一个纯粹的“事件实体”在世界中广播
                    Entity eventEntity = ECSManager.Instance.CreateEntity();
                    eventEntity.AddComponent(new ScoreEventComponent(stats.EnemyDeathScore));
                }
                
                // 玩家死亡逻辑保持不变...
                if (entity.HasComponent<PlayerTag>())
                {
                    Debug.Log("游戏结束！");
                    Time.timeScale = 0; 
                    EventManager.Broadcast(new GameOverEvent());
                }
                
                // HealthSystem 只负责生命周期终结
                ECSManager.Instance.DestroyEntity(entity);
            }
        }
    }
}