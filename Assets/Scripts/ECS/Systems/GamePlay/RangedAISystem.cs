// 路径: Assets/Scripts/ECS/Systems/GamePlay/RangedAISystem.cs
using System.Collections.Generic;
using UnityEngine;

public class RangedAISystem : SystemBase
{
    public RangedAISystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var rangedEnemies = GetEntitiesWith<RangedAIComponent, PositionComponent>(); // 注意：不再强依赖 WeaponComponent
        var player = ECSManager.Instance.PlayerEntity;

        if (player == null || !player.IsAlive) return;
        var pPos = player.GetComponent<PositionComponent>();

        for (int i = rangedEnemies.Count - 1; i >= 0; i--)
        {
            var enemy = rangedEnemies[i];

            if (enemy.HasComponent<ShootPrepStateComponent>() || 
                enemy.HasComponent<HitRecoveryComponent>() || 
                enemy.HasComponent<KnockbackComponent>())
            {
                continue;
            }

            var ai = enemy.GetComponent<RangedAIComponent>();
            var weapon = enemy.GetComponent<WeaponComponent>();
            
            // 【核心修复】：增加 weapon != null 的判空，防止崩溃切断ECS循环
            if (weapon != null && weapon.CurrentCooldown > 0) continue; 

            var ePos = enemy.GetComponent<PositionComponent>();
            Vector2 toPlayer = new Vector2(pPos.X - ePos.X, pPos.Y - ePos.Y);

            float attackOffset = (Mathf.PerlinNoise(enemy.GetHashCode(), Time.time * 0.4f) * 2f - 1f) * 1.5f;
            float dynamicAttackRange = ai.AttackRange + attackOffset;

            if (toPlayer.magnitude <= dynamicAttackRange)
            {
                Vector2 aimDir = toPlayer.normalized;

                enemy.AddComponent(new ShootPrepStateComponent(ai.PrepDuration, aimDir));
                
                // 赋予红外线射线（若射线太细，可把 0.1f 改成 0.2f）
                enemy.AddComponent(new DashPreviewIntentComponent(aimDir, 15f, 0.1f));

                if (enemy.HasComponent<MoveInputComponent>())
                {
                    var moveInput = enemy.GetComponent<MoveInputComponent>();
                    moveInput.X = 0; 
                    moveInput.Y = 0;
                }
            }
        }
    }
}