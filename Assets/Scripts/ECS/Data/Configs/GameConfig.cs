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
    public Dictionary<string, BulletData> BulletRecipes = new Dictionary<string, BulletData>();
    
    [Header("Wave Settings")]
    public List<WaveData> Waves = new List<WaveData>();
}