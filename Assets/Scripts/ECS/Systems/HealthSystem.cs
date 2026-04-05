using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : SystemBase
{
    public HealthSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 筛选拥有血量和视图的实体
        var entities = GetEntitiesWith<HealthComponent, ViewComponent>();
        
        for (int i = entities.Count - 1; i >= 0; i--)
        {
            var entity = entities[i];
            var health = entity.GetComponent<HealthComponent>();
            
            if (health.CurrentHealth <= 0)
            {
                // 使用 EnemyTag 识别敌人死亡并计分
                if (entity.HasComponent<EnemyTag>())
                {
                    ECSManager.Instance.Score += ECSManager.Instance.Config.EnemyKillScore;
                }
                
                // 使用 PlayerTag 识别玩家死亡
                if (entity.HasComponent<PlayerTag>())
                {
                    Debug.Log("游戏结束！");
                    Time.timeScale = 0;
                    UIManager.Instance.ShowGameOver();
                }
                
                ECSManager.Instance.DestroyEntity(entity);
            }
        }
    }
}