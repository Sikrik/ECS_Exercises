[System.Serializable]
public class EnemyData {
    public string Id;
    public int Level; // 【新增】敌人具体等级
    public float Health;
    public float Speed;
    public int Damage;
    public float HitRecoveryDuration;
    public string[] Traits;
    public int EnemyDeathScore;
    public float BounceForce;
    
    // --- AI/技能参数 ---
    public float FireRate;        
    public float ActionDist1;     
    public float ActionDist2;     
    public float ActionDist3;     
    public float ActionTime1;     
    public float SkillSpeed;      
    public float SkillDuration;   
    public float SkillCD;         
}