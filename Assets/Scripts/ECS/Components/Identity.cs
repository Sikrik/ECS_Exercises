// RangedAttackSystem.cs 新增
using System.Collections.Generic;
public class PlayerTag : Component { }
public class EnemyTag : Component { }
public class BulletTag : Component { }
public class BouncyTag : Component { }      // 标记实体具有弹性碰撞行为
public class NeedsBakingTag : Component { } // 标记实体需要初始化物理数据
public class RangedTag : Component { }