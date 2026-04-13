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

            // ==========================================
            // 【核心修复】：只要是近战角色，直接跳过！
            // 彻底切断生成 FireIntentComponent，绝对不会再开枪！
            // ==========================================
            if (player.HasComponent<MeleeCombatComponent>()) continue;

            var input = player.GetComponent<ShootInputComponent>();
            var pos = player.GetComponent<PositionComponent>();
            var weapon = player.GetComponent<WeaponComponent>();

            // CPU 优化：武器冷却中直接跳过索敌
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