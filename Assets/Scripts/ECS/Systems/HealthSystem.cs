// HealthSystem.cs 完整代码
using System.Collections.Generic;
using UnityEngine;
public class HealthSystem : SystemBase
{
    public HealthSystem(List<Entity> entities) : base(entities) { }
    public override void Update(float deltaTime)
    {
        // 筛选出所有有血量的实体
        var entities = GetEntitiesWith<HealthComponent, ViewComponent>();
        
        // 倒序遍历，防止删除元素出错
        for (int i = entities.Count - 1; i >= 0; i--)
        {
            var entity = entities[i];
            var health = entity.GetComponent<HealthComponent>();
            
            // 血量小于等于0，实体死亡
            if (health.CurrentHealth <= 0)
            {
                // 如果是敌人死亡，从配置读取得分，累加至全局得分，消除硬编码10
                if (entity.HasComponent<EnemyComponent>())
                {
                    ECSManager.Instance.Score += ECSManager.Instance.Config.EnemyKillScore;
                }
                
                // 如果是玩家死亡，触发游戏结束
                if (entity.HasComponent<PlayerComponent>())
                {
                    Debug.Log("游戏结束！");
                    Time.timeScale = 0;
                    UIManager.Instance.ShowGameOver(); // 调用UI显示游戏结束
                }
                // 统一销毁实体（使用ECSManager的统一方法，避免内存泄漏）
                ECSManager.Instance.DestroyEntity(entity);
            }
        }
    }
}