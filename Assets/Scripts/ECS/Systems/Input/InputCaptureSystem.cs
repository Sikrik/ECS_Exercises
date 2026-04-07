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

        // 2. 捕捉切换子弹指令
        if (Input.GetKeyDown(KeyCode.Alpha1)) PlayerShootingSystem.CurrentBulletType = BulletType.Normal;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) PlayerShootingSystem.CurrentBulletType = BulletType.Slow;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) PlayerShootingSystem.CurrentBulletType = BulletType.ChainLightning;
        else if (Input.GetKeyDown(KeyCode.Alpha4)) PlayerShootingSystem.CurrentBulletType = BulletType.AOE;
        
    }
}