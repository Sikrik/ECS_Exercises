// 路径: Assets/Scripts/ECS/Systems/Combat/ImpactResolutionSystem.cs
using System.Collections.Generic;

public class ImpactResolutionSystem : SystemBase
{
    public ImpactResolutionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var hitEvents = GetEntitiesWith<CollisionEventComponent>();

        foreach (var entity in hitEvents)
        {
            var evt = entity.GetComponent<CollisionEventComponent>();
            var source = evt.Source; // 撞击源（子弹）
            var target = evt.Target; // 被撞者（敌人）

            if (source == null || !source.IsAlive) continue;
            if (target == null || !target.IsAlive) continue;

            // ==========================================
            // 【核心修复 3】：正确生成子弹的伤害事件
            // ==========================================
            if (source.HasComponent<BulletTag>() && source.HasComponent<DamageComponent>())
            {
                float actualDmg = source.GetComponent<DamageComponent>().Value;

                // 强制要求使用对象初始化器，确保 DamageAmount 不为 0
                target.AddComponent(new DamageEventComponent { 
                    DamageAmount = actualDmg, 
                    Source = source, // 将子弹实体作为伤害源传入
                    IsCritical = false 
                });
            }
        }
    }
}