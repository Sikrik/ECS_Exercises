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
            
            // 核心逻辑：检测死亡
            if (health.CurrentHealth <= 0)
            {
                // 1. 如果是敌人死亡：增加得分
                if (entity.HasComponent<EnemyTag>())
                {
                    ECSManager.Instance.Score += ECSManager.Instance.Config.EnemyKillScore;
                }
                
                // 2. 如果是玩家死亡：暂停游戏并显示 UI
                if (entity.HasComponent<PlayerTag>())
                {
                    Debug.Log("游戏结束！");
                    Time.timeScale = 0; // 暂停所有物理和逻辑更新
                    UIManager.Instance.ShowGameOver(); // 弹出 UI
                }
                
                // 3. 统一通过 ECSManager 销毁实体数据及视觉物体
                ECSManager.Instance.DestroyEntity(entity);
            }
        }
    }
}