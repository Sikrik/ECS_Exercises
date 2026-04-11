[System.Serializable]
public class BulletData 
{
    public string Id;
    public float Speed;
    public float Damage;
    public float LifeTime;
    public float ShootInterval;
    
    // 特殊参数
    public float SlowRatio;
    public float SlowDuration;
    public int ChainTargets;
    public float ChainRange;
    public float AOERadius;

    // ======== 新增字段以支持 ComponentRegistry 动态装配 ========
    public string[] Traits; 
}