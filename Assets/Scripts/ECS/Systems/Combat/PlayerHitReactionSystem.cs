using System.Collections.Generic;

/// <summary>
/// 玩家受击反应系统（纯逻辑层）
/// 职责：处理受击后的无敌状态，并触发血量 UI 刷新
/// </summary>
public class PlayerHitReactionSystem : SystemBase
{
    public PlayerHitReactionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 抓取本帧受了伤的玩家
        var players = GetEntitiesWith<PlayerTag, DamageTakenEventComponent, HealthComponent>();
        
        foreach (var p in players)
        {
            // 如果不在无敌状态
            if (!p.HasComponent<InvincibleComponent>())
            {
                // 1. 赋予无敌状态
                p.AddComponent(new InvincibleComponent { Duration = ECSManager.Instance.Config.PlayerInvincibleDuration });
                
                // 2. 【核心修改】：不再发送 EventManager 委托，直接给玩家贴上一个“血量需刷新”的单帧标签
                if (!p.HasComponent<UIHealthUpdateEvent>())
                {
                    p.AddComponent(new UIHealthUpdateEvent());
                }
            }
        }
    }
}