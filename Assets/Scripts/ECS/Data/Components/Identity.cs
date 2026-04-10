// RangedAttackSystem.cs 新增
using System.Collections.Generic;

/// <summary>
/// 玩家标签组件，用于标识玩家实体
/// </summary>
public class PlayerTag : Component { }

/// <summary>
/// 敌人标签组件，用于标识敌方实体
/// </summary>
public class EnemyTag : Component { }

/// <summary>
/// 子弹标签组件，用于标识子弹实体
/// </summary>
public class BulletTag : Component { }

/// <summary>
/// 弹性碰撞标签组件，用于标记具有弹性碰撞行为的实体
/// </summary>
public class BouncyTag : Component { }

/// <summary>
/// 需要烘焙标签组件，用于标记需要初始化物理数据的实体
/// </summary>
public class NeedsBakingTag : Component { }

/// <summary>
/// 远程攻击标签组件，用于标识具有远程攻击能力的实体
/// </summary>
public class RangedTag : Component { }

// 新增：阵营枚举，用于未来区分伤害判定
public enum FactionType { Player, Enemy, Neutral }

// 新增：阵营组件，给所有实体打上阵营标签
public class FactionComponent : Component 
{
    public FactionType Value;
    public FactionComponent(FactionType type) => Value = type;
}
