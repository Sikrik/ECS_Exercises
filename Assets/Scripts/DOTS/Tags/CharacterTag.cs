using Unity.Entities;

namespace DOTS.Tags
{
    // 标记玩家
public struct PlayerTag : IComponentData { }

// 标记敌人
public struct EnemyTag : IComponentData { }

// 标记子弹
public struct BulletTag : IComponentData { }
}
