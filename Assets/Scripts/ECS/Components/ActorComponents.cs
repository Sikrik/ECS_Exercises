/// <summary>
/// 玩家组件，仅用于标记玩家实体
/// 是一个空的标记组件，没有额外数据
/// 这是区分玩家和其他实体的关键标识，玩家输入系统、相机跟随系统等核心功能都依赖此组件
/// </summary>
public class PlayerComponent : Component
{
    // 新增：玩家被撞后的无敌时间管理
    public float InvincibleTimer; 
    public float InvincibleDuration = 1.0f; // 默认无敌1秒
}
// 先放枚举，在类的外面
public enum BulletType
{
    Normal,     // 普通子弹
    Slow,       // 减速子弹
    ChainLightning, // 连锁闪电
    AOE         // 范围伤害
}
/// <summary>
/// 子弹组件，用于标记子弹实体，并存储子弹的相关属性
/// 仅用于存储数据，不包含业务逻辑
/// 子弹系统是游戏战斗的核心，决定了伤害输出和战斗体验，所有武器系统都依赖此组件
/// </summary>
public class BulletComponent : Component 
{
    /// <summary>子弹类型，决定命中后的效果</summary>
    public BulletType Type;
    /// <summary>子弹的伤害值</summary>
    public float Damage;
    /// <summary>子弹的剩余存活时间</summary>
    public float LifeTime;
}

// 新增：敌人类型枚举
public enum EnemyType
{
    Normal, // 普通敌人
    Fast,   // 快速敌人（速度快，血量少）
    Tank    // 坦克敌人（速度慢，血量高）
}
/// <summary>
/// 敌人组件，存储敌人的AI行为、战斗属性和移动参数
/// 包含攻击冷却、移动速度、击退状态等完整的行为数据
/// 敌人是游戏的主要挑战来源，AI系统、战斗系统、波次管理系统都依赖此组件
/// </summary>
public class EnemyComponent : Component
{
    /// <summary>敌人的攻击力，对玩家造成伤害时使用</summary>
    public int Damage;
    /// <summary>攻击冷却时间（秒），控制攻击频率</summary>
    public float AttackCooldown;
    /// <summary>当前攻击冷却剩余时间（秒）</summary>
    public float CurrentCooldown;
    /// <summary>敌人类型，决定敌人的行为模式和属性差异</summary>
    public EnemyType Type;
    /// <summary>敌人的移动速度，影响追击玩家的快慢</summary>
    public float MoveSpeed;
    
    /// <summary>碰撞击退方向X分量，用于击退效果计算</summary>
    public float KnockbackDirX;
    /// <summary>碰撞击退方向Y分量，用于击退效果计算</summary>
    public float KnockbackDirY;
    /// <summary>击退速度大小，决定击退效果的强度</summary>
    public float KnockbackSpeed;
    /// <summary>击退效果剩余时间（秒），倒计时结束后恢复正常移动</summary>
    public float KnockbackTimer;
    /// <summary>受击硬直剩余时间（秒），期间敌人无法行动</summary>
    public float HitRecoveryTimer;
}

/// <summary>
/// 弹性组件：标记该实体具有弹性，碰撞后会触发弹开（击退）效果
/// 这是一个标记组件，不包含任何数据
/// </summary>
public class BouncyComponent : Component 
{ 
}