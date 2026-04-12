using System.Collections.Generic;
using UnityEngine;

public class PlayerAimingSystem : SystemBase
{
    public static IAimStrategy CurrentAimStrategy = new AutoAimStrategy(); 

    public PlayerAimingSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 1. 查询条件加上 WeaponComponent
        var players = GetEntitiesWith<PlayerTag, ShootInputComponent, PositionComponent, WeaponComponent>();

        for (int i = players.Count - 1; i >= 0; i--)
        {
            var player = players[i];
            var input = player.GetComponent<ShootInputComponent>();
            var pos = player.GetComponent<PositionComponent>();
            var weapon = player.GetComponent<WeaponComponent>();

            // ==========================================
            // 【新增的 CPU 优化】：如果武器还在冷却中，直接跳过本帧索敌！
            // 这样能省下大量的 GridSystem 遍历性能
            // ==========================================
            if (weapon.CurrentCooldown > 0f) continue;

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