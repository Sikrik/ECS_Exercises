using System.Collections.Generic;
using UnityEngine;

public class DamageSystem : SystemBase
{
    public DamageSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 筛选出所有“发生了碰撞”且“具有伤害属性”的实体（如子弹、冲撞中的玩家）
        var attackers = GetEntitiesWith<CollisionEventComponent, DamageComponent>();

        foreach (var attacker in attackers)
        {
            var evt = attacker.GetComponent<CollisionEventComponent>();
            var dmg = attacker.GetComponent<DamageComponent>();

            // 目标必须存活且拥有血量组件
            if (evt.Target != null && evt.Target.IsAlive && evt.Target.HasComponent<HealthComponent>())
            {
                // 检查目标是否处于无敌状态
                if (!evt.Target.HasComponent<InvincibleComponent>())
                {
                    var health = evt.Target.GetComponent<HealthComponent>();
                    health.CurrentHealth -= dmg.Value;

                    // 如果受击目标是玩家，则根据配置添加无敌时间
                    if (evt.Target.HasComponent<PlayerTag>())
                    {
                        evt.Target.AddComponent(new InvincibleComponent 
                        { 
                            RemainingTime = ECSManager.Instance.Config.PlayerInvincibleDuration 
                        });
                        Debug.Log($"玩家受击！当前血量: {health.CurrentHealth}");
                    }
                }
            }
        }
    }
}