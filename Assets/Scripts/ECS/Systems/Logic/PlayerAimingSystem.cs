using System.Collections.Generic;
using UnityEngine;

public class PlayerAimingSystem : SystemBase
{
    private IAimStrategy _aimStrategy;

    public PlayerAimingSystem(List<Entity> entities) : base(entities) 
    {
        // 策略模式保留，用于处理鼠标辅助瞄准或自动瞄准
        _aimStrategy = new AutoAimStrategy(); 
    }

    public override void Update(float deltaTime)
    {
        var players = GetEntitiesWith<PlayerTag, InputComponent, PositionComponent>();

        for (int i = players.Count - 1; i >= 0; i--)
        {
            var player = players[i];
            var input = player.GetComponent<InputComponent>();
            var pos = player.GetComponent<PositionComponent>();

            // 如果玩家按下射击键，且当前没有尚未处理的开火意图
            if (input.IsShooting && !player.HasComponent<FireIntentComponent>())
            {
                // 计算瞄准方向
                Vector2 aimDir = _aimStrategy.GetAimDirection(player, input.MouseWorldPosition, Entities);
                
                // 贴上单帧意图标签
                player.AddComponent(new FireIntentComponent(aimDir));
            }
        }
        ReturnListToPool(players);
    }
}