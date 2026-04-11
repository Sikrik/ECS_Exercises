// 路径: Assets/Scripts/ECS/Systems/GamePlay/RangedAISystem.cs
using System.Collections.Generic;
using UnityEngine;

public class RangedAISystem : SystemBase
{
    public float AttackRange = 7f; // 索敌与射击半径

    public RangedAISystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 筛选拥有远程标签和武器的实体
        var rangedEnemies = GetEntitiesWith<RangedTag, PositionComponent, WeaponComponent>();
        var player = ECSManager.Instance.PlayerEntity;

        if (player == null || !player.IsAlive) return;
        var pPos = player.GetComponent<PositionComponent>();

        foreach (var enemy in rangedEnemies)
        {
            // 如果处于硬直或击退状态，停止思考
            if (enemy.HasComponent<HitRecoveryComponent>() || enemy.HasComponent<KnockbackComponent>())
            {
                continue;
            }

            var ePos = enemy.GetComponent<PositionComponent>();
            var weapon = enemy.GetComponent<WeaponComponent>();
            
            Vector2 toPlayer = new Vector2(pPos.X - ePos.X, pPos.Y - ePos.Y);
            float distanceSq = toPlayer.sqrMagnitude;

            // 如果进入射程
            if (distanceSq <= AttackRange * AttackRange)
            {
                // 1. 覆盖 EnemyTrackingSystem 的寻路，强行停车
                if (enemy.HasComponent<MoveInputComponent>())
                {
                    var moveInput = enemy.GetComponent<MoveInputComponent>();
                    moveInput.X = 0;
                    moveInput.Y = 0;
                }

                // 2. 如果武器冷却完毕，下达单帧开火意图
                if (weapon.CurrentCooldown <= 0 && !enemy.HasComponent<FireIntentComponent>())
                {
                    enemy.AddComponent(new FireIntentComponent(toPlayer.normalized));
                }
            }
            // 如果不在射程内，EnemyTrackingSystem 会自动继续赋值 MoveInputComponent 靠近玩家
        }
        
        ReturnListToPool(rangedEnemies);
    }
}