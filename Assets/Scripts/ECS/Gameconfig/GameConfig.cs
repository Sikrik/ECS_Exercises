using System;
using UnityEngine;

[Serializable]
public class GameConfig
{
    [Header("Player Settings")]
    public float PlayerMaxHealth = 100f;
    public float PlayerMoveSpeed = 5f;
    public float PlayerCollisionRadius = 0.5f;
    public float PlayerInvincibleDuration = 1.0f;

    [Header("Bullet Base Settings")]
    public float BulletSpeed = 10f;
    public float BulletDamage = 10f;
    public float BulletLifeTime = 2.0f;
    public float BulletCollisionRadius = 0.2f;
    public float ShootInterval = 0.2f;

    [Header("Slow Bullet Settings")]
    public float SlowBulletShootInterval = 0.4f;
    public float SlowRatio = 0.5f;        // 减速比例 (0-1)
    public float SlowDuration = 2.0f;     // 减速持续时间

    [Header("Chain Lightning Settings")]
    public float ChainLightningShootInterval = 0.6f;
    public int ChainTargets = 3;          // 闪电链最大跳跃目标数
    public float ChainRange = 5.0f;       // 闪电链跳跃范围
    public float ChainDamage = 8.0f;      // 闪电链每跳伤害

    [Header("AOE Bullet Settings")]
    public float AOEBulletShootInterval = 0.8f;
    public float AOERadius = 3.0f;        // 爆炸半径
    public float AOEDamage = 15.0f;       // 爆炸伤害

    [Header("Enemy Base Settings")]
    public float EnemyMaxHealth = 30f;
    public float EnemyMoveSpeed = 2f;
    public int EnemyDamage = 10;
    public float EnemyAttackCooldown = 1.0f;
    public float EnemyCollisionRadius = 0.4f;
    public float EnemyKnockbackSpeed = 5f;
    public float EnemyKnockbackDuration = 0.2f;
    public float EnemyHitRecoveryDuration = 0.4f; // 受击硬直时长

    [Header("Game Loop Settings")]
    public float InitialSpawnInterval = 2.0f;
    public float MinSpawnInterval = 0.5f;
    public float SpawnIntervalDecrease = 0.01f;
    public int EnemyKillScore = 10;

    [Header("Special Enemy: Fast")]
    public float FastEnemyMaxHealth = 15f;
    public float FastEnemySpeed = 4f;
    public float FastEnemyCollisionRadius = 0.3f;
    public float FastEnemyKnockbackSpeed = 7f;
    public float FastEnemyKnockbackDuration = 0.15f;

    [Header("Special Enemy: Tank")]
    public float TankEnemyMaxHealth = 100f;
    public float TankEnemySpeed = 1f;
    public float TankEnemyCollisionRadius = 0.6f;
    public float TankEnemyKnockbackSpeed = 2f;
    public float TankEnemyKnockbackDuration = 0.1f;
    // --- 核心修复：添加/重命名以下两个字段 ---
    public float NormalEnemyKnockbackSpeed = 5f;    
    public float NormalEnemyKnockbackDuration = 0.2f;  
}