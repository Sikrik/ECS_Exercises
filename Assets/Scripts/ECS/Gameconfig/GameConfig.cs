using System;
using UnityEngine;

[Serializable]
public class GameConfig
{
    [Header("Player Settings")]
    public float PlayerMaxHealth;
    public float PlayerMoveSpeed;
    public float PlayerCollisionRadius;
    public float PlayerInvincibleDuration;

    [Header("Bullet Settings")]
    public float BulletSpeed;
    public float BulletDamage;
    public float BulletLifeTime;
    public float BulletCollisionRadius;
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
    public float EnemyCollisionRadius;
    public float EnemyHitRecoveryDuration;
    public float NormalEnemyKnockbackSpeed;
    public float NormalEnemyKnockbackDuration;

    [Header("Fast Enemy Settings")]
    public float FastEnemyMaxHealth;
    public float FastEnemySpeed;
    public float FastEnemyCollisionRadius;
    public float FastEnemyKnockbackSpeed;
    public float FastEnemyKnockbackDuration;

    [Header("Tank Enemy Settings")]
    public float TankEnemyMaxHealth;
    public float TankEnemySpeed;
    public float TankEnemyCollisionRadius;
    public float TankEnemyKnockbackSpeed;
    public float TankEnemyKnockbackDuration;

    [Header("Spawn Settings")]
    public float InitialSpawnInterval;
    public float MinSpawnInterval;
    public float SpawnIntervalDecrease;
    public int EnemyKillScore;
}