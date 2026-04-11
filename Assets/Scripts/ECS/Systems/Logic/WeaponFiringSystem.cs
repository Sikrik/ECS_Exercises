using System.Collections.Generic;
using UnityEngine;

public class WeaponFiringSystem : SystemBase
{
    private GridSystem _gridSystem; // 如果你的子弹工厂需要注册到网格

    public WeaponFiringSystem(List<Entity> entities, GridSystem gridSystem) : base(entities) 
    {
        _gridSystem = gridSystem;
    }

    public override void Update(float deltaTime)
    {
        // 高内聚查询：只关心具备武器、位置和开火意图的实体（不管你是玩家还是敌人！）
        var firingEntities = GetEntitiesWith<WeaponComponent, FireIntentComponent, PositionComponent>();

        for (int i = firingEntities.Count - 1; i >= 0; i--)
        {
            var entity = firingEntities[i];
            var weapon = entity.GetComponent<WeaponComponent>();
            var intent = entity.GetComponent<FireIntentComponent>();
            var pos = entity.GetComponent<PositionComponent>();
            
            // 尝试获取阵营（如果没有阵营，默认算中立或玩家，视你现有逻辑而定）
            var factionComp = entity.GetComponent<FactionComponent>();
            FactionType faction = factionComp != null ? factionComp.Value : FactionType.Player;

            // 审核：冷却完毕才能开火
            if (weapon.CurrentCooldown <= 0f)
            {
                // 1. 调用通用工厂生成子弹
                // 注意：这里需要传入你的 Config，高内聚做法是通过事件或参数传入，这里演示标准调用
                var config = ConfigLoader.Load(); // 或者从全局上下文获取
                
                Entity bullet = BulletFactory.Create(config, weapon.CurrentBulletType, pos.Value, intent.AimDirection, faction);
                
                // 可选：如果需要注册到网格
                if (_gridSystem != null) _gridSystem.AddEntity(bullet);

                // 2. 重置武器冷却
                weapon.CurrentCooldown = weapon.FireRate;
            }

            // 无论这一帧是否开火成功（可能因为 CD 没好），单帧意图都会被消耗/抹除
            entity.RemoveComponent<FireIntentComponent>();
        }
        ReturnListToPool(firingEntities);
    }
}