// 新增：EnemyBuilderExtensions.cs

using UnityEngine;

public static class EnemyBuilderExtensions
{
    // 第一步：打身份标签 (Identity)
    public static Entity AsEnemy(this Entity entity)
    {
        entity.AddComponent(new EnemyTag()); //
        return entity;
    }

    // 第二步：装载基础表现组件 (Base View & Transform)
    public static Entity WithBaseView(this Entity entity, GameObject go, GameObject prefab, Vector3 pos)
    {
        entity.AddComponent(new ViewComponent(go, prefab)); //
        entity.AddComponent(new PositionComponent(pos.x, pos.y, 0)); //
        entity.AddComponent(new VelocityComponent(0, 0)); //
        entity.AddComponent(new NeedsBakingTag()); // 用于 PhysicsBakingSystem 烘焙
        entity.AddComponent(new CollisionFilterComponent(UnityEngine.LayerMask.GetMask("Player"))); //
        return entity;
    }

    // 第三步：装载核心战斗属性 (Core Combat)
    public static Entity WithCombatStats(this Entity entity, float hp, float speed, int damage)
    {
        entity.AddComponent(new HealthComponent(hp)); //
        entity.AddComponent(new EnemyStatsComponent { //
            MoveSpeed = speed, 
            Damage = damage 
        });
        return entity;
    }

    // 第四步：装载进阶/特殊组件 (Advanced Traits)
    public static Entity WithBouncy(this Entity entity)
    {
        entity.AddComponent(new BouncyTag()); // 标记后 KnockbackSystem 才会执行反弹
        return entity;
    }

    // 未来可以轻松添加更多进阶组件，例如自爆、远程攻击等
    // public static Entity WithExplosion(this Entity entity, float radius) { ... }
}