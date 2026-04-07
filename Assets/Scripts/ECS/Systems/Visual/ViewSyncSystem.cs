using System.Collections.Generic;
using UnityEngine;

public class ViewSyncSystem : SystemBase
{
    public ViewSyncSystem(List<Entity> entities) : base(entities) { }
    
    public override void Update(float deltaTime)
    {
        // 仅同步同时拥有位置和视图的实体
        var entities = GetEntitiesWith<PositionComponent, ViewComponent>();
        
        foreach (var entity in entities)
        {
            var pos = entity.GetComponent<PositionComponent>();
            var view = entity.GetComponent<ViewComponent>();
            
            if (view.GameObject != null)
            {
                // 将 ECS 的纯数据坐标同步给 GameObject
                view.GameObject.transform.position = new Vector3(pos.X, pos.Y, pos.Z);
            }
        }
        ReturnListToPool(entities);
    }
}