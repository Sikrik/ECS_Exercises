// 新建文件: Assets/Scripts/ECS/Data/Configs/Schemas/PlayerData.cs
[System.Serializable]
public class PlayerData 
{
    public string Id;
    public float MaxHealth;
    public float MoveSpeed;
    public float InvincibleDuration; // 受击无敌时间
    public float Mass;               // 质量（决定惯性和被撞击的反馈）
    public float FireRate;           // 射击间隔
    public float DashSpeed;          // 冲刺速度
    public float DashDuration;       // 冲刺持续时间
    public float DashCD;             // 冲刺冷却
    public string DefaultBullet;     // 初始武器配置
}