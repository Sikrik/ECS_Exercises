// RangedAttackSystem.cs 新增
using System.Collections.Generic;
public class PlayerTag : Component { }
public class EnemyTag : Component { }
public class BulletTag : Component { }
public class BouncyTag : Component { }      // 标记实体具有弹性碰撞行为
public class NeedsBakingTag : Component { } // 标记实体需要初始化物理数据
// 这是一个标记组件，没有任何数据，只代表“该实体等待回收”
public class DestroyTag : Component { }

// IdentityComponents.cs 增加一行
public class RangedTag : Component { }


public class RangedAttackSystem : SystemBase
{
    public RangedAttackSystem(List<Entity> entities) : base(entities) { }
    public override void Update(float deltaTime)
    {
        // 高内聚：该系统只处理带有 RangedTag 的实体
        var entities = GetEntitiesWith<RangedTag, PositionComponent>();
        foreach (var e in entities)
        {
            // 执行远程射击逻辑...
        }
    }
}