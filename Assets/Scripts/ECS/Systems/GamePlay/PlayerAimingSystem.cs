// 路径: Assets/Scripts/ECS/Systems/GamePlay/PlayerAimingSystem.cs
using System.Collections.Generic;
using UnityEngine;

public class PlayerAimingSystem : SystemBase
{
    public static IAimStrategy CurrentAimStrategy = new AutoAimStrategy(); 

    public PlayerAimingSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var players = GetEntitiesWith<PlayerTag, ShootInputComponent, PositionComponent, WeaponComponent>();

        for (int i = players.Count - 1; i >= 0; i--)
        {
            var player = players[i];
            var input = player.GetComponent<ShootInputComponent>();

            // 【核心修复 1】：去掉所有 Melee拦截 和 Cooldown拦截！
            // 必须让系统每帧计算方向，UI 箭头和血量环才能丝滑跟随旋转！
            if (!player.HasComponent<FireIntentComponent>())
            {
                Vector2? aimDir = CurrentAimStrategy.GetAimDirection(player, input, ECSManager.Instance.Grid);
                
                if (aimDir.HasValue)
                {
                    player.AddComponent(new FireIntentComponent(aimDir.Value));
                }
            }
        }
    }
}