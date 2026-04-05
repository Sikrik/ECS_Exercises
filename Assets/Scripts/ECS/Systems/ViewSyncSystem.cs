using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 视图同步系统，负责将ECS中的数据同步到Unity的GameObject视图中
/// 主要是将实体的位置同步到GameObject的Transform上
/// </summary>
public class ViewSyncSystem : SystemBase
{
    /// <summary>
    /// 初始化视图同步系统
    /// </summary>
    /// <param name="entities">系统可处理的实体列表</param>
    public ViewSyncSystem(List<Entity> entities) : base(entities) { }
    
    /// <summary>
    /// 每帧更新，将ECS的位置数据同步到视图对象
    /// </summary>
    /// <param name="deltaTime">上一帧到当前帧的时间间隔</param>
    public override void Update(float deltaTime)
    {
        // 筛选出所有同时拥有位置和视图组件的实体
        var entities = GetEntitiesWith<PositionComponent, ViewComponent>();
        
        foreach (var entity in entities)
        {
            var pos = entity.GetComponent<PositionComponent>();
            var view = entity.GetComponent<ViewComponent>();
            
            // 把ECS里的位置，同步到GameObject的Transform上
            view.GameObject.transform.position = new Vector3(pos.X, pos.Y, pos.Z);
        }
    }
}