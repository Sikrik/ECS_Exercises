// 路径: Assets/Scripts/ECS/Data/Configs/GameConfig.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]

public class GameConfig
{
    [Header("Physics & Bounce Settings")]
    public float CollisionPushDistance = 0.2f; 
    public float CollisionBounceForce = 5.0f;
    public float KnockbackFriction = 15.0f; // 新增

    [Header("Spawn Settings")]
    public float InitialSpawnInterval;

    [Header("Enemy Growth Settings")] 
    public float EnemyHpGrowth = 0.2f;    
    public float EnemyDmgGrowth = 0.15f;  
    public float EnemySpeedGrowth = 0.05f;

    [Header("Visual Settings")]
    public float GhostFadeSpeed = 3.5f;     // 新增
    public float GhostInitialAlpha = 0.6f;  // 新增

    [Header("Data Recipes")]
    public Dictionary<string, PlayerData> PlayerRecipes = new Dictionary<string, PlayerData>();
    public Dictionary<string, EnemyData> EnemyRecipes = new Dictionary<string, EnemyData>();
    public Dictionary<string, BulletData> BulletRecipes = new Dictionary<string, BulletData>(); // 新增子弹配方表
    
    [Header("Upgrade & Level Settings")]
    public Dictionary<string, UpgradeData> UpgradeRecipes = new Dictionary<string, UpgradeData>();
    public Dictionary<int, int> LevelExpRecipes = new Dictionary<int, int>(); 
    
    [Header("Wave Settings")]
    public List<WaveData> Waves = new List<WaveData>();
}