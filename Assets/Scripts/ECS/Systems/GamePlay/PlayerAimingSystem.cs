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

            // 1. 近战角色不需要远程索敌和开火意图，直接安全跳过！
            if (player.HasComponent<MeleeCombatComponent>()) continue;

            var input = player.GetComponent<ShootInputComponent>();
            var weapon = player.GetComponent<WeaponComponent>();

            // 2. 武器冷却中跳过
            if (weapon.CurrentCooldown > 0f) continue;

            // 3. 纯粹只负责生成开火意图，【绝对不碰任何视觉旋转】！
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