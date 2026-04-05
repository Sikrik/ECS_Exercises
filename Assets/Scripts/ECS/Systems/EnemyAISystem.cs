// EnemyAISystem.cs 优化版本
// 优化内容：
// 1. 移除重复的位置更新逻辑，统一交由MovementSystem处理
// 2. 职责分离，AI系统仅负责设置速度，不再处理位置更新
using System.Collections.Generic;
using UnityEngine;
public class EnemyAISystem : SystemBase
{
    public EnemyAISystem(List<Entity> entities) : base(entities) { }
    public override void Update(float deltaTime)
    {
        // 游戏暂停时不执行AI逻辑
        if (Time.timeScale <= 0) return;
        
        UpdateEnemyMovement(deltaTime);
    }
    /// <summary>
    /// 更新所有敌人的移动逻辑（核心AI）
    /// </summary>
    void UpdateEnemyMovement(float deltaTime)
    {
        GameConfig config = ECSManager.Instance.Config;
        if (config == null) return;
        // 获取玩家实体（空值防御）
        Entity player = ECSManager.Instance.PlayerEntity;
        if (player == null) return;
        
        PositionComponent playerPos = player.GetComponent<PositionComponent>();
        if (playerPos == null) return;
        // 遍历所有敌人实体
        List<Entity> enemies = GetEntitiesWith<EnemyComponent, PositionComponent, VelocityComponent>();
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            Entity enemy = enemies[i];
            if (enemy == null) continue;
            // 获取敌人核心组件（空值防御）
            EnemyComponent enemyComp = enemy.GetComponent<EnemyComponent>();
            PositionComponent enemyPos = enemy.GetComponent<PositionComponent>();
            VelocityComponent enemyVel = enemy.GetComponent<VelocityComponent>();
            if (enemyComp == null || enemyPos == null || enemyVel == null) continue;
            
            // ========== 根据敌人类型获取对应移动速度 ==========
            float moveSpeed;
            switch (enemyComp.Type)
            {
                case EnemyType.Fast:
                    // 快速敌人：使用快速敌人专属速度
                    moveSpeed = config.FastEnemySpeed;
                    break;
                case EnemyType.Tank:
                    // 坦克敌人：使用坦克敌人专属速度
                    moveSpeed = config.TankEnemySpeed;
                    break;
                case EnemyType.Normal:
                default:
                    // 普通敌人：使用默认速度
                    moveSpeed = config.EnemyMoveSpeed;
                    break;
            }
            
            // 处理击退阶段
            if (enemyComp.KnockbackTimer > 0)
            {
                enemyComp.KnockbackTimer -= deltaTime;
                // 速度线性衰减到0
                float progress = enemyComp.KnockbackTimer / config.EnemyKnockbackDuration;
                float currentSpeed = enemyComp.KnockbackSpeed * progress;
                
                // 仅设置速度，位置更新交由MovementSystem统一处理
                enemyVel.X = enemyComp.KnockbackDirX * currentSpeed;
                enemyVel.Y = enemyComp.KnockbackDirY * currentSpeed;
                
                // 跳过正常AI逻辑
                continue;
            }
            
            // 处理恢复阶段
            if (enemyComp.HitRecoveryTimer > 0)
            {
                enemyComp.HitRecoveryTimer -= deltaTime;
                // 速度线性恢复到正常
                float progress = 1 - (enemyComp.HitRecoveryTimer / config.EnemyHitRecoveryDuration);
                
                // 计算正常的AI方向（变量重命名避免与正常阶段的变量冲突）
                float recoveryDirX = playerPos.X - enemyPos.X;
                float recoveryDirY = playerPos.Y - enemyPos.Y;
                float recoveryMag = Mathf.Sqrt(recoveryDirX * recoveryDirX + recoveryDirY * recoveryDirY);
                
                if (recoveryMag > 0.1f)
                {
                    recoveryDirX /= recoveryMag;
                    recoveryDirY /= recoveryMag;
                    // 应用恢复进度，速度从0升到正常，仅设置速度
                    float currentSpeed = moveSpeed * progress;
                    enemyVel.X = recoveryDirX * currentSpeed;
                    enemyVel.Y = recoveryDirY * currentSpeed;
                }
                else
                {
                    enemyVel.X = 0;
                    enemyVel.Y = 0;
                }
                
                // 跳过正常AI逻辑
                continue;
            }
            
            // ========== 正常AI移动逻辑 ==========
            // 计算敌人朝向玩家的方向（归一化，避免斜向移动更快）
            float dirX = playerPos.X - enemyPos.X;
            float dirY = playerPos.Y - enemyPos.Y;
            float mag = Mathf.Sqrt(dirX * dirX + dirY * dirY);
            
            // 避免除以0（敌人和玩家重合时不移动）
            if (mag > 0.1f)
            {
                dirX /= mag;
                dirY /= mag;
                // 更新敌人速度（按类型速度移动，支持X/Y访问）
                // 仅设置速度，位置更新交由MovementSystem统一处理
                enemyVel.X = dirX * moveSpeed;
                enemyVel.Y = dirY * moveSpeed;
            }
            else
            {
                // 靠近玩家时停止移动
                enemyVel.X = 0;
                enemyVel.Y = 0;
            }
        }

    }
}