using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家输入系统：负责处理键盘输入并转化为物理速度
/// 重构要点：使用 PlayerTag 标记定位玩家
/// </summary>
public class PlayerInputSystem : SystemBase
{
    public PlayerInputSystem(List<Entity> entities) : base(entities) { }
    
    public override void Update(float deltaTime)
    {
        // 1. 筛选出玩家实体：必须拥有 PlayerTag 和 VelocityComponent
        var playerEntities = GetEntitiesWith<PlayerTag, VelocityComponent>();
        if (playerEntities.Count == 0) return;
        
        var player = playerEntities[0];
        var vel = player.GetComponent<VelocityComponent>();
        
        // 2. 处理移动输入
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");
        
        // 归一化输入向量，防止斜向移动过快
        Vector2 inputDir = new Vector2(inputX, inputY).normalized;
        
        // 从全局配置获取玩家移动速度
        float moveSpeed = ECSManager.Instance.Config.PlayerMoveSpeed; 
        
        // 更新速度组件数据（由 MovementSystem 负责最终位移计算）
        vel.VX = inputDir.x * moveSpeed;
        vel.VY = inputDir.y * moveSpeed;
        
        // 3. 处理子弹类型切换输入 (保留原有逻辑)
        HandleBulletSwitching();
    }
    
    private void HandleBulletSwitching()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PlayerShootingSystem.CurrentBulletType = BulletType.Normal;
            Debug.Log("切换到普通子弹");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlayerShootingSystem.CurrentBulletType = BulletType.Slow;
            Debug.Log("切换到减速子弹");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            PlayerShootingSystem.CurrentBulletType = BulletType.ChainLightning;
            Debug.Log("切换到连锁闪电子弹");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            PlayerShootingSystem.CurrentBulletType = BulletType.AOE;
            Debug.Log("切换到范围伤害子弹");
        }
    }
}