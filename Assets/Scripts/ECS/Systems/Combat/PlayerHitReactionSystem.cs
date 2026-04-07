using System.Collections.Generic;

public class PlayerHitReactionSystem : SystemBase
{
    public PlayerHitReactionSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 只抓取本帧受了伤的玩家
        var players = GetEntitiesWith<PlayerTag, DamageTakenEventComponent, HealthComponent>();
        
        foreach (var p in players)
        {
            if (!p.HasComponent<InvincibleComponent>())
            {
                // 1. 给玩家贴上无敌标签
                p.AddComponent(new InvincibleComponent { Duration = ECSManager.Instance.Config.PlayerInvincibleDuration });
                
                // 2. 广播扣血 UI 事件
                var health = p.GetComponent<HealthComponent>();
                EventManager.Broadcast(new PlayerHealthChangedEvent { CurrentHealth = health.CurrentHealth, MaxHealth = health.MaxHealth });
            }
        }
    }
}