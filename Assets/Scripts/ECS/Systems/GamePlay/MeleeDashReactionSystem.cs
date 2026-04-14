// 路径: Assets/Scripts/ECS/Systems/Combat/MeleeDashReactionSystem.cs
using System.Collections.Generic;

public class MeleeDashReactionSystem : SystemBase
{
    public MeleeDashReactionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 只筛选同时拥有 近战组件 + 刚刚开始冲刺事件 的实体
        var entities = GetEntitiesWith<MeleeCombatComponent, DashStartedEventComponent>();
        
        foreach (var e in entities)
        {
            // 给自己挂上挥砍意图，让之前写好的 MeleeExecutionSystem 去完美执行
            e.AddComponent(new MeleeSwingIntentComponent { 
                RadiusMultiplier = 1.5f, 
                AngleOverride = 360f 
            });
        }
    }
}