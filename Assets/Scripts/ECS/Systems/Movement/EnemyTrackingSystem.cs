using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敌人寻路系统（AI）
/// 架构优化：AI 不再直接操作物理速度，而是输出“移动意图（MoveInput）”
/// </summary>
public class EnemyTrackingSystem : SystemBase
{
    public EnemyTrackingSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var enemies = GetEntitiesWith<EnemyTag, PositionComponent>();
        var player = ECSManager.Instance.PlayerEntity;

        if (player == null || !player.IsAlive) return;

        var pPos = player.GetComponent<PositionComponent>();

        foreach (var enemy in enemies)
        {
            // 1. 如果怪物正处于硬直或击退状态，AI 暂停思考（保持原有逻辑）
            if (enemy.HasComponent<KnockbackComponent>() || enemy.HasComponent<HitRecoveryComponent>())
            {
                continue;
            }

            var ePos = enemy.GetComponent<PositionComponent>();
            
            // 2. 计算向玩家移动的方向
            Vector2 dir = new Vector2(pPos.X - ePos.X, pPos.Y - ePos.Y);
            if (dir.sqrMagnitude > 0.001f) 
            {
                dir.Normalize();
            }

            // 3. 【核心重构】：写入意图组件，而不是直接改速度！
            if (!enemy.HasComponent<MoveInputComponent>())
            {
                enemy.AddComponent(new MoveInputComponent(dir.x, dir.y));
            }
            else
            {
                var input = enemy.GetComponent<MoveInputComponent>();
                input.X = dir.x;
                input.Y = dir.y;
            }
        }
        ReturnListToPool(enemies);
    }
}