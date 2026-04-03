// 新增：敌人类型枚举
public enum EnemyType
{
    Normal, // 普通敌人
    Fast,   // 快速敌人（速度快，血量少）
    Tank    // 坦克敌人（速度慢，血量高）
}
public class EnemyComponent : Component
{
    public int Damage;
    public float AttackCooldown;
    public float CurrentCooldown;
    // 新增：敌人类型
    public EnemyType Type;
    
    // 碰撞击退与恢复状态
    public float KnockbackDirX;
    public float KnockbackDirY;
    public float KnockbackSpeed;
    public float KnockbackTimer;
    public float HitRecoveryTimer;
}