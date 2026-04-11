using System.Collections.Generic;
using UnityEngine;

public class PlayerAimingSystem : SystemBase
{
    // 改为静态公开，替代掉老系统的那个静态变量
    public static IAimStrategy CurrentAimStrategy = new AutoAimStrategy(); 

    public PlayerAimingSystem(List<Entity> entities) : base(entities) 
    {
        // 移除构造函数里的初始化，改用上面的静态默认值
    }

    public override void Update(float deltaTime)
    {
        var players = GetEntitiesWith<PlayerTag, ShootInputComponent, PositionComponent>();

        for (int i = players.Count - 1; i >= 0; i--)
        {
            var player = players[i];
            var input = player.GetComponent<ShootInputComponent>();
            var pos = player.GetComponent<PositionComponent>();

            if (input.IsShooting && !player.HasComponent<FireIntentComponent>())
            {
                // 使用静态的 CurrentAimStrategy
                Vector2? aimDir = CurrentAimStrategy.GetAimDirection(player, input, ECSManager.Instance.Grid);
                
                if (aimDir.HasValue)
                {
                    player.AddComponent(new FireIntentComponent(aimDir.Value));
                }
            }
        }
        ReturnListToPool(players);
    }
}