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
                // 1. 获取当前出战角色的字符串 ID
                string classId = ECSManager.Instance.SelectedCharacter.ToString();

// 2. 尝试从字典中获取配方数据
                float invincibleTime = 1f; // 给一个安全兜底值
                if (ECSManager.Instance.Config.PlayerRecipes.TryGetValue(classId, out var recipe))
                {
                    invincibleTime = recipe.InvincibleDuration;
                }

// 3. 赋予无敌状态
                p.AddComponent(new InvincibleComponent { Duration = invincibleTime });
                
                // 2. 【核心修改】：不再发送 EventManager 委托，直接给玩家贴上一个“血量需刷新”的单帧标签
                if (!p.HasComponent<UIHealthUpdateEvent>())
                {
                    p.AddComponent(new UIHealthUpdateEvent());
                }
            }
        }
    }
}