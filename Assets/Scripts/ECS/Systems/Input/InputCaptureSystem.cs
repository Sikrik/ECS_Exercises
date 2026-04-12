using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 输入捕捉系统 (位于 InitializationSystemGroup)
/// 职责：将 Unity 的原生输入转化为 ECS 的意图组件（数据层）。
/// 修复说明：对接了全新的武器组件系统和 PlayerAimingSystem，去除了对废弃的 PlayerShootingSystem 的依赖。
/// </summary>
public class InputCaptureSystem : SystemBase
{
    public InputCaptureSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var players = GetEntitiesWith<PlayerTag>();
        if (players.Count == 0) return;

        var player = players[0];
        
        // ==========================================
        // 1. 捕捉位移意图
        // ==========================================
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        
        if (player.HasComponent<MoveInputComponent>())
        {
            var input = player.GetComponent<MoveInputComponent>();
            input.X = x; input.Y = y;
        }
        else
        {
            player.AddComponent(new MoveInputComponent(x, y));
        }

        // ==========================================
        // 2. 捕捉鼠标状态 (持续捕捉，由策略决定是否采用)
        // ==========================================
        bool isShooting = Input.GetMouseButton(0); 
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (player.HasComponent<ShootInputComponent>())
        {
            var shootInput = player.GetComponent<ShootInputComponent>();
            shootInput.IsShooting = isShooting;
            shootInput.TargetX = mouseWorldPos.x;
            shootInput.TargetY = mouseWorldPos.y;
        }
        else
        {
            player.AddComponent(new ShootInputComponent 
            { 
                IsShooting = isShooting, 
                TargetX = mouseWorldPos.x, 
                TargetY = mouseWorldPos.y 
            });
        }

        // ==========================================
        // 3. 捕捉切换子弹指令 (直接修改玩家的 WeaponComponent)
        // ==========================================
        if (player.HasComponent<WeaponComponent>())
        {
            var weapon = player.GetComponent<WeaponComponent>();
            if (Input.GetKeyDown(KeyCode.Alpha1)) weapon.CurrentBulletType = BulletType.Normal;
            else if (Input.GetKeyDown(KeyCode.Alpha2)) weapon.CurrentBulletType = BulletType.Slow;
            else if (Input.GetKeyDown(KeyCode.Alpha3)) weapon.CurrentBulletType = BulletType.ChainLightning;
            else if (Input.GetKeyDown(KeyCode.Alpha4)) weapon.CurrentBulletType = BulletType.AOE;
        }
        
        // ==========================================
        // 4. 按 Tab 键随时切换自动/手动瞄准模式 (对接 PlayerAimingSystem)
        // ==========================================
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (PlayerAimingSystem.CurrentAimStrategy is AutoAimStrategy)
            {
                PlayerAimingSystem.CurrentAimStrategy = new ManualAimStrategy();
                Debug.Log("已切换为：鼠标手动瞄准模式");
            }
            else
            {
                PlayerAimingSystem.CurrentAimStrategy = new AutoAimStrategy();
                Debug.Log("已切换为：全自动索敌模式");
            }
        }
        
        // ==========================================
        // 5. 捕捉冲刺指令 (单帧意图)
        // ==========================================
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (!player.HasComponent<DashInputComponent>())
            {
                player.AddComponent(new DashInputComponent());
            }
        }
    }
}