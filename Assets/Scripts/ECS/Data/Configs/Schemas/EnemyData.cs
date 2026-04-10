[System.Serializable]
public class EnemyData {
    public string Id;
    public float Health;
    public float Speed;
    public int Damage;
    public float HitRecoveryDuration; // 新增：从 CSV 读取硬直时间
    public string[] Traits;
    public int EnemyDeathScore;
    // --- 新增这个字段 ---
    public float BounceForce;
}