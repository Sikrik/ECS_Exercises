using System;

[Serializable]
public class EnemyData
{
    public string Id;          // 对应 CSV 的第一列
    public float Health;
    public float Speed;
    public int Damage;
    public string[] Traits;    // 组件清单，例如 ["Bouncy", "Ranged"]
}