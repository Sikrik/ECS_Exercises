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
/// 物理烘焙标签：标记实体需要初始化物理碰撞器数据 (交由 Simulation 组处理)
/// </summary>
public class NeedsPhysicsBakingTag : Component { }

/// <summary>
/// 视觉烘焙标签：标记实体需要缓存 SpriteRenderer 和初始颜色 (交由 Presentation 组处理)
/// </summary>
public class NeedsVisualBakingTag : Component { }

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
/// <summary>
/// 死亡状态标签：表示该实体逻辑上已经死亡，但还在等待结算（如发奖金、播放死亡动画等）
/// </summary>
public class DeadTag : Component { }