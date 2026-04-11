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
    public float PlayerMass;          // 新增
    public float PlayerFireRate;      // 新增
    public float PlayerDashSpeed;     // 新增
    public float PlayerDashDuration;  // 新增
    public float PlayerDashCD;        // 新增

    [Header("Physics & Bounce Settings")]
    public float CollisionPushDistance = 0.2f; 
    public float CollisionBounceForce = 5.0f;

    [Header("Spawn Settings")]
    public float InitialSpawnInterval;

    // 核心数据字典：用于存储从 CSV 加载的具体配方
    [Header("Data Recipes")]
    public Dictionary<string, PlayerData> PlayerRecipes = new Dictionary<string, PlayerData>();
    public Dictionary<string, EnemyData> EnemyRecipes = new Dictionary<string, EnemyData>();
    public Dictionary<string, BulletData> BulletRecipes = new Dictionary<string, BulletData>();
}