using System.Collections.Generic;
using UnityEngine;

public class PlayerDeathSystem : SystemBase
{
    public PlayerDeathSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var deadPlayers = GetEntitiesWith<DeadTag, PlayerTag>();
        
        if (deadPlayers.Count > 0)
        {
            Debug.Log("玩家死亡，抛出全局结束事件！");
            Time.timeScale = 0; 
            
            // 抛出单帧事件供 UI 消费
            var eventEntity = ECSManager.Instance.CreateEntity();
            eventEntity.AddComponent(new GameOverEventComponent());

            // 移除 PlayerTag 防止重复触发
            deadPlayers[0].RemoveComponent<PlayerTag>();
        }
        ReturnListToPool(deadPlayers);
    }
}