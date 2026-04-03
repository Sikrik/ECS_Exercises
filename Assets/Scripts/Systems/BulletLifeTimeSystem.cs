// BulletLifeTimeSystem.cs 修复后版本
// 修复内容：
// 1. 移除重复的GameObject销毁逻辑，避免与DestroyEntity的对象池回收逻辑冲突
// 2. 修复了已销毁对象被错误回收到对象池的问题
using System.Collections.Generic;
using UnityEngine;
public class BulletLifeTimeSystem : SystemBase
{
    public BulletLifeTimeSystem(List<Entity> entities) : base(entities) { }
    
    public override void Update(float deltaTime)
    {
        var bullets = GetEntitiesWith<BulletComponent>();
        
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            var bullet = bullets[i];
            var bulletComp = bullet.GetComponent<BulletComponent>();
            
            bulletComp.LifeTime -= deltaTime;
            
            if (bulletComp.LifeTime <= 0)
            {
                // 修复：移除重复的Object.Destroy，DestroyEntity会自动处理对象池回收/销毁
                ECSManager.Instance.DestroyEntity(bullet);
            }
        }
    }
}