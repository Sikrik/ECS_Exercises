// 路径: Assets/Scripts/ECS/Systems/GamePlay/ShootPrepSystem.cs
using System.Collections.Generic;

public class ShootPrepSystem : SystemBase
{
    public ShootPrepSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var preps = GetEntitiesWith<ShootPrepStateComponent>();
        
        for (int i = preps.Count - 1; i >= 0; i--)
        {
            var e = preps[i];

            // 打断逻辑
            if (e.HasComponent<DeadTag>() || e.HasComponent<HitRecoveryComponent>() || e.HasComponent<KnockbackComponent>())
            {
                e.RemoveComponent<ShootPrepStateComponent>();
                e.RemoveComponent<AimLineIntentComponent>();
                continue;
            }

            var prep = e.GetComponent<ShootPrepStateComponent>();
            prep.Timer -= deltaTime;
            
            // 👇【高内聚改造】：删除了强行清空 MoveInput 和 Velocity 的越权代码
            // 刹车逻辑已全部移交 MovementSystem 仲裁！

            if (prep.Timer <= 0)
            {
                e.RemoveComponent<ShootPrepStateComponent>();
                e.RemoveComponent<AimLineIntentComponent>();
                e.AddComponent(new FireIntentComponent(prep.TargetDir)); 
            }
        }
    }
}