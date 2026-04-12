using System.Collections.Generic;
using UnityEngine;

public class CameraCullingSystem : SystemBase
{
    private Camera _mainCam;
    private float _padding = 2.0f; // 安全缓冲距离，防止在边缘频繁闪烁

    public CameraCullingSystem(List<Entity> entities) : base(entities) 
    {
        _mainCam = Camera.main;
    }

    public override void Update(float deltaTime)
    {
        if (_mainCam == null) return;

        // 计算相机的世界坐标可视边界
        float camHeight = _mainCam.orthographicSize * 2f;
        float camWidth = camHeight * _mainCam.aspect;
        Vector3 camPos = _mainCam.transform.position;

        float minX = camPos.x - camWidth / 2f - _padding;
        float maxX = camPos.x + camWidth / 2f + _padding;
        float minY = camPos.y - camHeight / 2f - _padding;
        float maxY = camPos.y + camHeight / 2f + _padding;

        // 遍历所有有视觉表现的实体
        var entities = GetEntitiesWith<PositionComponent, ViewComponent>();

        for (int i = entities.Count - 1; i >= 0; i--)
        {
            var e = entities[i];
            var pos = e.GetComponent<PositionComponent>();
            var view = e.GetComponent<ViewComponent>();
            
            if (view.SpriteRenderer == null) continue;

            bool isOffScreen = pos.X < minX || pos.X > maxX || pos.Y < minY || pos.Y > maxY;

            if (isOffScreen && !e.HasComponent<OffScreenTag>())
            {
                e.AddComponent(new OffScreenTag());
                view.SpriteRenderer.enabled = false; // 屏幕外直接关闭渲染
            }
            else if (!isOffScreen && e.HasComponent<OffScreenTag>())
            {
                e.RemoveComponent<OffScreenTag>();
                view.SpriteRenderer.enabled = true;  // 回到屏幕内恢复渲染
            }
        }
    }
}