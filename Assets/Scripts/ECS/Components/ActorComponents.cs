
/// <summary>
/// 子弹属性组件：存储子弹的静态战斗数据
/// </summary>
public class BulletStatsComponent : Component 
{
    public BulletType Type;
    public float Damage;
}


public class EnemyStatsComponent : Component {
    public EnemyType Type;
    public float MoveSpeed;
    public int Damage;
    public float AttackCooldown;
    public float HitRecoveryDuration; // 新增：存储实体的运行时硬直数值
}




