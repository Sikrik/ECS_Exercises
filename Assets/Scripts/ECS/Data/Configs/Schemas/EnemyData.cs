[System.Serializable]
public class EnemyData {
    public string Id;
    public float Health;
    public float Speed;
    public int Damage;
    public float HitRecoveryDuration;
    public string[] Traits;
    public int EnemyDeathScore;
    public float BounceForce;
    
    // --- 新增扩展 AI/技能 参数 ---
    public float FireRate;        // 射击间隔 (Ranged专用)
    public float ActionDist1;     // AI判定距离1 (Charger冲锋触发距离 / Ranged风筝距离)
    public float ActionDist2;     // AI判定距离2 (Ranged容差)
    public float ActionDist3;     // AI判定距离3 (Ranged攻击距离)
    public float ActionTime1;     // AI时间参数1 (Ranged蓄力时间)
    public float SkillSpeed;      // 技能速度 (Charger冲刺速度)
    public float SkillDuration;   // 技能持续时间 (Charger冲刺时间)
    public float SkillCD;         // 技能冷却 (Charger冲刺冷却)
}