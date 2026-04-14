using System.Collections.Generic;
using UnityEngine;

public class CameraFollowSystem : SystemBase
{
    private Camera _mainCam;
    
    // SmoothDamp 需要的状态变量
    private Vector3 _velocity = Vector3.zero; 
    
    // 到达目标的平滑时间（越小跟得越紧，越大越平滑但有拖拽感，0.1f~0.2f 是防晕的最佳区间）
    private float _smoothTime = 0.1f; 

    public CameraFollowSystem(List<Entity> entities) : base(entities) 
    {
        _mainCam = Camera.main;
    }

    public override void Update(float deltaTime)
    {
        if (_mainCam == null) return;

        var players = GetEntitiesWith<PlayerTag, PositionComponent>();
        if (players.Count > 0)
        {
            var pos = players[0].GetComponent<PositionComponent>();
            Vector3 target = new Vector3(pos.X, pos.Y, _mainCam.transform.position.z);
            
            // 绝杀抖动：使用带阻尼的 SmoothDamp，并且严格传入 deltaTime
            _mainCam.transform.position = Vector3.SmoothDamp(
                _mainCam.transform.position, 
                target, 
                ref _velocity, 
                _smoothTime,
                Mathf.Infinity, // 最大速度限制（不限）
                deltaTime       // 抹平帧率波动的关键！
            );
        }
    }
}