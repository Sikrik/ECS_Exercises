
/// <summary>
/// 子弹属性组件：存储子弹的静态战斗数据
/// </summary>
public class BulletStatsComponent : Component 
{
    public BulletType Type;
    public float Damage;
}



/// <summary>
/// 敌人基础属性：存储不会轻易改变的数值
/// </summary>
public class EnemyStatsComponent : Component
{
    public EnemyType Type;
    public float MoveSpeed;
    public int Damage;
    public float AttackCooldown;
}





