using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 玩家输入系统，负责处理玩家的键盘输入
/// 处理玩家的移动输入，更新玩家的速度
/// </summary>
public class PlayerInputSystem : SystemBase
{
    /// <summary>
    /// 初始化玩家输入系统
    /// </summary>
    /// <param name="entities">系统可处理的实体列表</param>
    public PlayerInputSystem(List<Entity> entities) : base(entities) { }
    
    /// <summary>
    /// 每帧更新，处理玩家的输入
    /// </summary>
    /// <param name="deltaTime">上一帧到当前帧的时间间隔</param>
    public override void Update(float deltaTime)
    {
        var playerEntities = GetEntitiesWith<PlayerComponent, VelocityComponent>();
        if (playerEntities.Count == 0) return;
        
        var player = playerEntities[0];
        var vel = player.GetComponent<VelocityComponent>();
        
        // 移动输入（保留原有逻辑）
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");
        Vector2 inputDir = new Vector2(inputX, inputY).normalized;
        
        float moveSpeed = ECSManager.Instance.Config.PlayerMoveSpeed; 
        vel.SpeedX = inputDir.x * moveSpeed;
        vel.SpeedY = inputDir.y * moveSpeed;
        
        // 子弹类型切换输入
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