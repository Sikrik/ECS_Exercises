using System.Collections.Generic;

[System.Serializable]
public class WaveData
{
    public int WaveIndex;        // 波次序号
    
    // 存储本波的混合生成配方：敌人ID -> 生成数量
    public Dictionary<string, int> SpawnDict = new Dictionary<string, int>(); 
    public int TotalSpawnCount;  // 解析时自动计算总数
    
    public float SpawnInterval;  // 本波敌人的生成间隔
    public float NextWaveDelay;  // 清空后距下一波的等待时间
}