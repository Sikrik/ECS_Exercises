using System.Collections.Generic;
using UnityEngine;

public class CollisionSystem : SystemBase
{
    public CollisionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 先更新无敌计时器
        UpdateInvincibleTimer(deltaTime);
        
        // 执行碰撞检测
        CheckEnemyPlayerCollision();
        // 注意：你的原有拾取碰撞逻辑请保留，这里我移除了我假设的拾取代码，避免符号错误
        // CheckPlayerPickupCollision();
    }

    /// <summary>
    /// 新增：更新玩家的无敌计时器
    /// </summary>
    void UpdateInvincibleTimer(float deltaTime)
    {
        var player = ECSManager.Instance?.PlayerEntity;
        if (player == null) return;
        
        var playerHealth = player.GetComponent<HealthComponent>();
        if (playerHealth == null) return;
        
        if (playerHealth.InvincibleTimer > 0)
        {
            playerHealth.InvincibleTimer -= deltaTime;
        }
    }

    /// <summary>
    /// 检测玩家与敌人的碰撞
    /// </summary>
    void CheckEnemyPlayerCollision()
    {
        var player = ECSManager.Instance?.PlayerEntity;
        if (player == null) return;
        
        // 先获取配置，避免未声明的问题
        var config = ECSManager.Instance.Config;
        
        var playerPos = player.GetComponent<PositionComponent>();
        var playerCol = player.GetComponent<CollisionComponent>();
        var playerHealth = player.GetComponent<HealthComponent>();
        if (playerPos == null || playerCol == null || playerHealth == null) return;
        
        // 新增：无敌帧判断，如果玩家处于无敌状态，直接跳过碰撞
        if (playerHealth.InvincibleTimer > 0) return;
        
        var enemies = GetEntitiesWith<EnemyComponent, PositionComponent, CollisionComponent>();
        
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            var enemy = enemies[i];
            if (enemy == null) continue;
            
            var enemyPos = enemy.GetComponent<PositionComponent>();
            var enemyCol = enemy.GetComponent<CollisionComponent>();
            var enemyComp = enemy.GetComponent<EnemyComponent>();
            if (enemyPos == null || enemyCol == null || enemyComp == null) continue;
            
            enemyComp.CurrentCooldown -= Time.deltaTime;
            if (enemyComp.CurrentCooldown > 0) continue;
            
            // 优化：平方距离比较，避免开根号
            float dx = playerPos.X - enemyPos.X;
            float dy = playerPos.Y - enemyPos.Y;
            float distSq = dx * dx + dy * dy;
            float radiusSum = playerCol.Radius + enemyCol.Radius;
            
            if (distSq < radiusSum * radiusSum)
            {
                playerHealth.CurrentHealth -= enemyComp.Damage;
                // 新增：受伤后初始化无敌计时器
                if (config != null)
                {
                    playerHealth.InvincibleTimer = config.PlayerInvincibleDuration;
                }
                
                enemyComp.CurrentCooldown = enemyComp.AttackCooldown;
                
                // 初始化击退与恢复状态
                float dirX = enemyPos.X - playerPos.X;
                float dirY = enemyPos.Y - playerPos.Y;
                float dirMag = Mathf.Sqrt(dirX * dirX + dirY * dirY);
                if (dirMag > 0.1f)
                {
                    dirX /= dirMag;
                    dirY /= dirMag;
                }
                
                if (config != null)
                {
                    // 根据敌人类型读取对应的击退配置
                    float knockbackSpeed, knockbackDuration, hitRecoveryDuration;
                    switch (enemyComp.Type)
                    {
                        case EnemyType.Fast:
                            knockbackSpeed = config.FastEnemyKnockbackSpeed;
                            knockbackDuration = config.FastEnemyKnockbackDuration;
                            hitRecoveryDuration = config.FastEnemyHitRecoveryDuration;
                            break;
                        case EnemyType.Tank:
                            knockbackSpeed = config.TankEnemyKnockbackSpeed;
                            knockbackDuration = config.TankEnemyKnockbackDuration;
                            hitRecoveryDuration = config.TankEnemyHitRecoveryDuration;
                            break;
                        case EnemyType.Normal:
                        default:
                            knockbackSpeed = config.NormalEnemyKnockbackSpeed;
                            knockbackDuration = config.NormalEnemyKnockbackDuration;
                            hitRecoveryDuration = config.NormalEnemyHitRecoveryDuration;
                            break;
                    }

                    // 初始化击退与恢复状态
                    enemyComp.KnockbackDirX = dirX;
                    enemyComp.KnockbackDirY = dirY;
                    enemyComp.KnockbackSpeed = knockbackSpeed;
                    enemyComp.KnockbackTimer = knockbackDuration;
                    enemyComp.HitRecoveryTimer = hitRecoveryDuration;
                }
            }
        }
    }

    // 注意：以下是我假设的拾取碰撞逻辑，你的项目里已经有自己的拾取逻辑了，请保留你原来的代码
    // /// <summary>
    // /// 检测玩家与拾取道具的碰撞
    // /// </summary>
    // void CheckPlayerPickupCollision()
    // {
    //     var player = ECSManager.Instance?.PlayerEntity;
    //     if (player == null) return;
    //     
    //     var playerPos = player.GetComponent<PositionComponent>();
    //     var playerCol = player.GetComponent<CollisionComponent>();
    //     if (playerPos == null || playerCol == null) return;
    //     
    //     var pickups = GetEntitiesWith<PickupComponent, PositionComponent, CollisionComponent>();
    //     
    //     for (int i = pickups.Count - 1; i >= 0; i--)
    //     {
    //         var pickup = pickups[i];
    //         if (pickup == null) continue;
    //         
    //         var pickupPos = pickup.GetComponent<PositionComponent>();
    //         var pickupCol = pickup.GetComponent<CollisionComponent>();
    //         var pickupComp = pickup.GetComponent<PickupComponent>();
    //         if (pickupPos == null || pickupCol == null || pickupComp == null) continue;
    //         
    //         // 优化：平方距离比较，避免开根号
    //         float dx = playerPos.X - pickupPos.X;
    //         float dy = playerPos.Y - pickupPos.Y;
    //         float distSq = dx * dx + dy * dy;
    //         float radiusSum = playerCol.Radius + pickupCol.Radius;
    //         
    //         if (distSq < radiusSum * radiusSum)
    //         {
    //             // 应用拾取效果
    //             pickupComp.Apply(player);
    //             // 销毁道具
    //             ECSManager.Instance.DestroyEntity(pickup);
    //         }
    //     }
    // }
}