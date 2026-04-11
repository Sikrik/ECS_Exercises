// 路径: Assets/Scripts/ECS/Data/Configs/Schemas/BulletData.cs
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

    // ======== 动态装配特性 ========
    public string[] Traits; 

    // ======== 新增：子弹的碰撞体积半径 ========
    public float HitRadius; 
}