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

    [Header("Spawn Settings")]
    public float InitialSpawnInterval;

    // 核心数据字典：用于存储从 CSV 加载的具体配方
    [Header("Data Recipes")]
    public Dictionary<string, PlayerData> PlayerRecipes = new Dictionary<string, PlayerData>();
    public Dictionary<string, EnemyData> EnemyRecipes = new Dictionary<string, EnemyData>();
    
    // 【新增】升级与经验曲线字典
    [Header("Upgrade & Level Settings")]
    public Dictionary<string, UpgradeData> UpgradeRecipes = new Dictionary<string, UpgradeData>();
    public Dictionary<int, int> LevelExpRecipes = new Dictionary<int, int>(); // Key: 当前等级, Value: 升下一级所需EXP
    
    [Header("Wave Settings")]
    public List<WaveData> Waves = new List<WaveData>();
}