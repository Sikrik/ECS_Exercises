public class EnemyStatsComponent : Component 
{
    public EnemyType Type;
    public EnemyData Config; // 直接引用配置表中的原始数据
    public float CurrentMoveSpeed; // 仅存储会动态变化的数值
    public int EnemyDeathScore => Config.EnemyDeathScore; // 通过属性访问，不占额外内存
}