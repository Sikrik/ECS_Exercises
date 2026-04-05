using System.Collections.Generic;
using UnityEngine;

public class CollisionSystem : SystemBase
{
    public CollisionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var playerEntity = ECSManager.Instance.PlayerEntity;
        if (playerEntity == null || !playerEntity.IsAlive) return;

        var pPos = playerEntity.GetComponent<PositionComponent>();
        var pCol = playerEntity.GetComponent<CollisionComponent>();
        var pComp = playerEntity.GetComponent<PlayerComponent>();
        var pHealth = playerEntity.GetComponent<HealthComponent>();

        // 更新玩家无敌时间
        if (pComp.InvincibleTimer > 0)
        {
            pComp.InvincibleTimer -= deltaTime;
            return; // 无敌时间内不检测碰撞伤害
        }

        // 获取所有敌人
        var enemies = GetEntitiesWith<EnemyComponent, PositionComponent, CollisionComponent>();

        foreach (var enemy in enemies)
        {
            if (!enemy.IsAlive) continue;

            var ePos = enemy.GetComponent<PositionComponent>();
            var eCol = enemy.GetComponent<CollisionComponent>();
            var eComp = enemy.GetComponent<EnemyComponent>();

            // 使用平方距离进行高性能碰撞检测
            float dx = pPos.X - ePos.X;
            float dy = pPos.Y - ePos.Y;
            float distSq = dx * dx + dy * dy;
            float radiusSum = pCol.Radius + eCol.Radius;

            if (distSq <= radiusSum * radiusSum)
            {
                // 发生碰撞，玩家扣血
                pHealth.CurrentHealth -= eComp.Damage;
                
                // 触发无敌时间，防止下一帧立即再次受伤
                pComp.InvincibleTimer = pComp.InvincibleDuration;

                Debug.Log($"玩家受到敌人碰撞伤害！剩余血量: {pHealth.CurrentHealth}");

                // 如果血量归零，逻辑由 HealthSystem 处理销毁
                if (pHealth.CurrentHealth <= 0) break;
            }
        }
    }
}