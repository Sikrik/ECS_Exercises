using System;
using UnityEngine;

[Serializable]
public class GameConfig
{
    [Header("Player Settings")]
    public float PlayerMaxHealth;
    public float PlayerMoveSpeed;
    public float PlayerInvincibleDuration;

    [Header("Physics & Bounce Settings")]
    public float CollisionPushDistance = 0.2f; 
    public float CollisionBounceForce = 5.0f;

    [Header("Bullet Settings")]
    public float BulletSpeed;
    public float BulletDamage;
    public float BulletLifeTime;
    public float ShootInterval;

    [Header("Special Bullet Settings")]
    public float SlowBulletShootInterval;
    public float SlowRatio;
    public float SlowDuration;
    public float ChainLightningShootInterval;
    public int ChainTargets;
    public float ChainRange;
    public float ChainDamage;
    public float AOEBulletShootInterval;
    public float AOERadius;
    public float AOEDamage;

    [Header("Enemy Base Settings")]
    public float EnemyMaxHealth;
    public float EnemyMoveSpeed;
    public int EnemyDamage;
    public float EnemyAttackCooldown;
    public float EnemyHitRecoveryDuration; 

    [Header("Fast Enemy Settings")] // --- 核心修复：补全缺失符号 ---
    public float FastEnemyMaxHealth;
    public float FastEnemySpeed;

    [Header("Tank Enemy Settings")] // --- 核心修复：补全缺失符号 ---
    public float TankEnemyMaxHealth;
    public float TankEnemySpeed;

    [Header("Spawn Settings")]
    public float InitialSpawnInterval;
    public int EnemyKillScore;
}