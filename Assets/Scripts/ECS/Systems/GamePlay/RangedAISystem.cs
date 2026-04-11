// 路径: Assets/Scripts/ECS/Systems/GamePlay/RangedAISystem.cs
using System.Collections.Generic;
using UnityEngine;

public class RangedAISystem : SystemBase
{
    public RangedAISystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var rangedEnemies = GetEntitiesWith<RangedAIComponent, PositionComponent, WeaponComponent>();
        var player = ECSManager.Instance.PlayerEntity;

        if (player == null || !player.IsAlive) return;
        var pPos = player.GetComponent<PositionComponent>();

        for (int i = rangedEnemies.Count - 1; i >= 0; i--)
        {
            var enemy = rangedEnemies[i];

            // 互斥判断：正在蓄力、硬直或被击退时，停止 AI 思考
            if (enemy.HasComponent<ShootPrepStateComponent>() || 
                enemy.HasComponent<HitRecoveryComponent>() || 
                enemy.HasComponent<KnockbackComponent>())
            {
                continue;
            }

            var ai = enemy.GetComponent<RangedAIComponent>();
            var weapon = enemy.GetComponent<WeaponComponent>();
            
            // 如果武器还在 CD 中，乖乖跟着 EnemyTrackingSystem 走，不触发红外线蓄力
            if (weapon.CurrentCooldown > 0) continue; 

            var ePos = enemy.GetComponent<PositionComponent>();
            Vector2 toPlayer = new Vector2(pPos.X - ePos.X, pPos.Y - ePos.Y);

            // 进入射程警戒范围
            if (toPlayer.magnitude <= ai.AttackRange)
            {
                Vector2 aimDir = toPlayer.normalized;

                // 1. 赋予蓄力状态组件 (锁定开火方向)
                enemy.AddComponent(new ShootPrepStateComponent(ai.PrepDuration, aimDir));
                
                // 2. 【核心复用】：赋予预览意图组件 (长度15米，宽度0.1米，复用现有的表现层渲染细长红外线)
                enemy.AddComponent(new DashPreviewIntentComponent(aimDir, 15f, 0.1f));

                // 3. 停止当前常规寻路移动
                if (enemy.HasComponent<MoveInputComponent>())
                {
                    var moveInput = enemy.GetComponent<MoveInputComponent>();
                    moveInput.X = 0; 
                    moveInput.Y = 0;
                }
            }
        }
        ReturnListToPool(rangedEnemies);
    }
}