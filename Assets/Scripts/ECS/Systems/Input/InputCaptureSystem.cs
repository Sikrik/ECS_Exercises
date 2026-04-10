using System.Collections.Generic;
using UnityEngine;

public class InputCaptureSystem : SystemBase
{
    public InputCaptureSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var players = GetEntitiesWith<PlayerTag>();
        if (players.Count == 0) return;

        var player = players[0];
        
        // 1. 捕捉位移意图
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

        // 2. 捕捉鼠标状态 (即使在自动瞄准模式下，我们也可以一直捕捉，只是策略用不用它的区别)
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

        // 3. 捕捉切换子弹指令
        if (Input.GetKeyDown(KeyCode.Alpha1)) PlayerShootingSystem.CurrentBulletType = BulletType.Normal;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) PlayerShootingSystem.CurrentBulletType = BulletType.Slow;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) PlayerShootingSystem.CurrentBulletType = BulletType.ChainLightning;
        else if (Input.GetKeyDown(KeyCode.Alpha4)) PlayerShootingSystem.CurrentBulletType = BulletType.AOE;
        
        // 4. 按 Tab 键随时切换自动/手动瞄准模式
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (PlayerShootingSystem.CurrentAimStrategy is AutoAimStrategy)
            {
                PlayerShootingSystem.CurrentAimStrategy = new ManualAimStrategy();
                Debug.Log("已切换为：鼠标手动瞄准模式");
            }
            else
            {
                PlayerShootingSystem.CurrentAimStrategy = new AutoAimStrategy();
                Debug.Log("已切换为：全自动索敌模式");
            }
        }
        
        // ==========================================
        // 5. 【新增】：捕捉冲刺指令
        // ==========================================
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (!player.HasComponent<DashInputComponent>())
            {
                player.AddComponent(new DashInputComponent());
            }
        }
        
        ReturnListToPool(players);
    }
}