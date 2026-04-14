// 路径: Assets/Scripts/ECS/Data/Configs/Schemas/WaveData.cs
using System.Collections.Generic;

[System.Serializable]
public struct EnemySpawnInfo 
{
    public string Id;
    public int Level;
    public int Count;
}

[System.Serializable]
public class WaveData
{
    public int WaveIndex;        // 波次序号
    
    // 存储本波的生成配方列表 (支持同一个怪物不同等级)
    public List<EnemySpawnInfo> SpawnList = new List<EnemySpawnInfo>(); 
    
    // 【优化】移除了未被消费的冗余字段 TotalSpawnCount
    
    public float SpawnInterval;  // 本波敌人的生成间隔
    public float NextWaveDelay;  // 清空后距下一波的等待时间
}