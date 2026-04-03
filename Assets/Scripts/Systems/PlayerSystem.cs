using System.Collections.Generic;
using UnityEngine;

public class PlayerSystem : SystemBase
{
    public PlayerSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var player = ECSManager.Instance.PlayerEntity;
        if (player == null) return;
        
        var playerHealth = player.GetComponent<HealthComponent>();
        if (playerHealth == null) return;
        
        // 新增：更新无敌计时器，处理闪烁效果
        // if (playerHealth.InvincibleTimer > 0)
        // {
        //     // 无敌期间玩家闪烁，增加视觉反馈
        //     var render = player.GetComponent<RenderComponent>();
        //     if (render != null)
        //     {
        //         render.enabled = Mathf.FloorToInt(Time.time * 10) % 2 == 0;
        //     }
        // }
        // else
        // {
        //     // 无敌结束后，恢复玩家的显示
        //     var render = player.GetComponent<RenderComponent>();
        //     if (render != null)
        //     {
        //         render.enabled = true;
        //     }
        // }
        
        // 原有玩家移动、射击等逻辑...
        UpdatePlayerMovement(deltaTime);
        UpdatePlayerShoot(deltaTime);
    }

    void UpdatePlayerMovement(float deltaTime)
    {
        // 你的原有玩家移动逻辑
    }

    void UpdatePlayerShoot(float deltaTime)
    {
        // 你的原有玩家射击逻辑
    }
}