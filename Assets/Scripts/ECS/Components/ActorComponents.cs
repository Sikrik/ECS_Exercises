public class EnemyStatsComponent : Component {
    public EnemyType Type;
    public float MoveSpeed;
    public int Damage;
    public float AttackCooldown;
    public float HitRecoveryDuration; 
    public float BaseMoveSpeed;
    
    // 👇新增这一行
    public int EnemyDeathScore; 
}