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
    public float KnockbackFriction = 15.0f; 

    [Header("Spawn Settings")]
    public float InitialSpawnInterval;

    [Header("Enemy Growth Settings")] 
    public float EnemyHpGrowth = 0.2f;    
    public float EnemyDmgGrowth = 0.15f;  
    public float EnemySpeedGrowth = 0.05f;

    [Header("Visual Settings")]
    public float GhostFadeSpeed = 3.5f;     
    public float GhostInitialAlpha = 0.6f;  

    [Header("Data Recipes")]
    public Dictionary<string, PlayerData> PlayerRecipes = new Dictionary<string, PlayerData>();
    public Dictionary<string, EnemyData> EnemyRecipes = new Dictionary<string, EnemyData>();
    public Dictionary<string, BulletData> BulletRecipes = new Dictionary<string, BulletData>(); 
    public Dictionary<string, TalentData> TalentRecipes = new Dictionary<string, TalentData>();
    
    [Header("Upgrade & Level Settings")]
    // 👇【核心修改】：拆分成远程和近战两个字典
    public Dictionary<string, UpgradeData> RangedUpgradeRecipes = new Dictionary<string, UpgradeData>();
    public Dictionary<string, UpgradeData> MeleeUpgradeRecipes = new Dictionary<string, UpgradeData>();
    
    public Dictionary<int, int> LevelExpRecipes = new Dictionary<int, int>(); 
    
    [Header("Wave Settings")]
    public List<WaveData> Waves = new List<WaveData>();
}