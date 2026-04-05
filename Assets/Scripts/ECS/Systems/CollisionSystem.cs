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
        var config = ECSManager.Instance.Config;

        // 更新玩家无敌时间计时
        if (pComp.InvincibleTimer > 0)
        {
            pComp.InvincibleTimer -= deltaTime;
        }

        // 获取所有具有位置和碰撞属性的敌人
        var enemies = GetEntitiesWith<EnemyComponent, PositionComponent, CollisionComponent>();

        foreach (var enemy in enemies)
        {
            if (!enemy.IsAlive) continue;

            var ePos = enemy.GetComponent<PositionComponent>();
            var eCol = enemy.GetComponent<CollisionComponent>();
            var eComp = enemy.GetComponent<EnemyComponent>();

            // 高性能平方距离碰撞检测
            float dx = ePos.X - pPos.X;
            float dy = ePos.Y - pPos.Y;
            float distSq = dx * dx + dy * dy;
            float radiusSum = pCol.Radius + eCol.Radius;

            if (distSq <= radiusSum * radiusSum)
            {
                // --- 逻辑1：碰撞伤害 (受无敌帧保护) ---
                if (pComp.InvincibleTimer <= 0)
                {
                    pHealth.CurrentHealth -= eComp.Damage;
                    pComp.InvincibleTimer = pComp.InvincibleDuration; 
                    Debug.Log($"玩家受击！剩余血量: {pHealth.CurrentHealth}");
                }

                // --- 逻辑2：条件弹开 (仅在对方有 BouncyComponent 时触发) ---
                if (enemy.HasComponent<BouncyComponent>())
                {
                    float mag = Mathf.Sqrt(distSq);
                    if (mag > 0.01f)
                    {
                        // 设置击退方向
                        eComp.KnockbackDirX = dx / mag;
                        eComp.KnockbackDirY = dy / mag;
                        
                        // 根据敌人类型从配置中读取击退参数
                        switch (eComp.Type)
                        {
                            case EnemyType.Fast:
                                eComp.KnockbackSpeed = config.FastEnemyKnockbackSpeed;
                                eComp.KnockbackTimer = config.FastEnemyKnockbackDuration;
                                break;
                            case EnemyType.Tank:
                                eComp.KnockbackSpeed = config.TankEnemyKnockbackSpeed;
                                eComp.KnockbackTimer = config.TankEnemyKnockbackDuration;
                                break;
                            default:
                                eComp.KnockbackSpeed = config.NormalEnemyKnockbackSpeed;
                                eComp.KnockbackTimer = config.NormalEnemyKnockbackDuration;
                                break;
                        }
                    }
                }
                else
                {
                    // 如果没有弹性组件，敌人速度在 AI 系统中不会被切换为击退速度
                    // 它们会继续贴着玩家尝试移动，产生“贴着”的效果
                }

                if (pHealth.CurrentHealth <= 0) break;
            }
        }
    }
}