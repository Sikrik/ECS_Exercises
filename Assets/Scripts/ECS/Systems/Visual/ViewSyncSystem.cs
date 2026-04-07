using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 视觉同步系统：将 ECS 逻辑坐标 (PositionComponent) 实时同步至 Unity 表现层 (Transform)
/// 优化点：直接使用烘焙阶段缓存的渲染器和对象，消除每帧的 GetComponent 开销
/// </summary>
public class ViewSyncSystem : SystemBase
{
    public ViewSyncSystem(List<Entity> entities) : base(entities) { }
    
    public override void Update(float deltaTime)
    {
        // 仅处理同时拥有位置数据和视觉实体的对象
        var entities = GetEntitiesWith<PositionComponent, ViewComponent>();
        
        foreach (var entity in entities)
        {
            var pos = entity.GetComponent<PositionComponent>();
            var view = entity.GetComponent<ViewComponent>();
            
            // 确保 GameObject 依然存在（防止异步销毁导致的空引用）
            if (view.GameObject != null)
            {
                // 将逻辑层的 X, Y, Z 数据一次性同步给 Unity Transform
                // 注意：保持 Z 轴同步以处理 2D 游戏中的层级遮盖关系
                view.GameObject.transform.position = new Vector3(pos.X, pos.Y, pos.Z);
            }
        }

        // 归还列表池，维持 0 GC
        ReturnListToPool(entities);
    }
}