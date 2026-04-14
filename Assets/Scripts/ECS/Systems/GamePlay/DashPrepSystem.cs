// 路径: Assets/Scripts/ECS/Systems/GamePlay/DashPrepSystem.cs
using System.Collections.Generic;

public class DashPrepSystem : SystemBase
{
    public DashPrepSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var preps = GetEntitiesWith<DashPrepStateComponent>();
        
        for (int i = preps.Count - 1; i >= 0; i--)
        {
            var e = preps[i];

            if (e.HasComponent<DeadTag>() || e.HasComponent<HitRecoveryComponent>() || e.HasComponent<KnockbackComponent>())
            {
                e.RemoveComponent<DashPrepStateComponent>();
                e.RemoveComponent<DashPreviewIntentComponent>();
                continue;
            }

            var prep = e.GetComponent<DashPrepStateComponent>();
            prep.Timer -= deltaTime;
            
            // 👇【高内聚改造】：同理，删除了越权操作 Velocity 和 MoveInput 的代码

            if (prep.Timer <= 0)
            {
                // 👇【高内聚改造】：甚至不需要在这里伪造 MoveInput 了！
                // 因为 DashActivationSystem 本来就会优先读取 DashPrepStateComponent.TargetDir
                e.RemoveComponent<DashPrepStateComponent>();
                e.RemoveComponent<DashPreviewIntentComponent>();
                e.AddComponent(new DashInputComponent()); 
            }
        }
    }
}