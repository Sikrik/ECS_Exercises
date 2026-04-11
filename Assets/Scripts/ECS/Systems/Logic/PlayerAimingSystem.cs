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
        // 修复1：将 InputComponent 替换为实际存在的 ShootInputComponent
        var players = GetEntitiesWith<PlayerTag, ShootInputComponent, PositionComponent>();

        for (int i = players.Count - 1; i >= 0; i--)
        {
            var player = players[i];
            var input = player.GetComponent<ShootInputComponent>();
            var pos = player.GetComponent<PositionComponent>();

            // 如果玩家按下射击键，且当前没有尚未处理的开火意图
            if (input.IsShooting && !player.HasComponent<FireIntentComponent>())
            {
                // 修复2：传入正确的参数 ECSManager.Instance.Grid，并接收 Vector2? 返回值
                Vector2? aimDir = _aimStrategy.GetAimDirection(player, input, ECSManager.Instance.Grid);
                
                // 修复3：检查是否有返回值（是否满足开火条件）
                if (aimDir.HasValue)
                {
                    // 贴上单帧意图标签
                    player.AddComponent(new FireIntentComponent(aimDir.Value));
                }
            }
        }
        ReturnListToPool(players);
    }
}