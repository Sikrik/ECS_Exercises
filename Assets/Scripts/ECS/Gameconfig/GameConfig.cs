using System;
using System.Collections.Generic;
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
    public Dictionary<string, EnemyData> EnemyRecipes = new Dictionary<string, EnemyData>();

    [Header("Spawn Settings")]
    public float InitialSpawnInterval;
    public int EnemyKillScore;
    
}