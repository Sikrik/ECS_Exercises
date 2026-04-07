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
                // 【修复点】直接给当前实体加分，不要创建 new Entity()
                if (entity.HasComponent<EnemyTag>() && entity.HasComponent<EnemyStatsComponent>())
                {
                    var stats = entity.GetComponent<EnemyStatsComponent>();
                    // 挂载计分组件，ScoreSystem 处理后，EntityCleanupSystem 会统一回收该实体
                    entity.AddComponent(new ScoreEventComponent(stats.EnemyDeathScore));
                }
                
                if (entity.HasComponent<PlayerTag>())
                {
                    Debug.Log("游戏结束！");
                    Time.timeScale = 0; 
                    EventManager.Broadcast(new GameOverEvent());
                }

                // 统一贴上待销毁标签
                if (!entity.HasComponent<PendingDestroyComponent>())
                {
                    entity.AddComponent(new PendingDestroyComponent());
                }
            }
        }
        ReturnListToPool(entities);
    }
}