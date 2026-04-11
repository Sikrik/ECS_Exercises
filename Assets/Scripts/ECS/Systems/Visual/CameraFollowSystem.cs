using System.Collections.Generic;
using UnityEngine;

public class CameraFollowSystem : SystemBase
{
    private Camera _mainCam;

    public CameraFollowSystem(List<Entity> entities) : base(entities) 
    {
        _mainCam = Camera.main;
    }

    public override void Update(float deltaTime)
    {
        if (_mainCam == null) return;

        // 仅查询需要被相机跟随的玩家坐标
        var players = GetEntitiesWith<PlayerTag, PositionComponent>();
        if (players.Count > 0)
        {
            var pos = players[0].GetComponent<PositionComponent>();
            Vector3 target = new Vector3(pos.X, pos.Y, _mainCam.transform.position.z);
            _mainCam.transform.position = Vector3.Lerp(_mainCam.transform.position, target, 0.1f);
        }
        ReturnListToPool(players);
    }
}