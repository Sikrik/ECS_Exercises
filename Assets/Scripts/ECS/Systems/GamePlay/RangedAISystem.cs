// 路径: Assets/Scripts/ECS/Systems/GamePlay/RangedAISystem.cs
using System.Collections.Generic;
using UnityEngine;

public class RangedAISystem : SystemBase
{
    public RangedAISystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var rangedEnemies = GetEntitiesWith<RangedAIComponent, PositionComponent>(); 
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
            
            // ==========================================
            // 👇 【核心修复】：逻辑纠正！没有武器 (null) 必须被拦截跳过！
            // ==========================================
            if (weapon == null || weapon.CurrentCooldown > 0) continue; 

            var ePos = enemy.GetComponent<PositionComponent>();
            Vector2 toPlayer = new Vector2(pPos.X - ePos.X, pPos.Y - ePos.Y);

            float attackOffset = (Mathf.PerlinNoise(enemy.GetHashCode(), Time.time * 0.4f) * 2f - 1f) * 1.5f;
            float dynamicAttackRange = ai.AttackRange + attackOffset;

            if (toPlayer.magnitude <= dynamicAttackRange)
            {
                Vector2 aimDir = toPlayer.normalized;

                enemy.AddComponent(new ShootPrepStateComponent(ai.PrepDuration, aimDir));
                
                // 赋予红外线射线（告知表现层：瞄准方向，长度 15 米，宽度 0.1 米）
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