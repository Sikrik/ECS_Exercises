using System.Collections.Generic;
using UnityEngine;

public class BulletCollisionSystem : SystemBase 
{
    private ContactFilter2D _filter;
    private RaycastHit2D[] _results = new RaycastHit2D[5];

    public BulletCollisionSystem(List<Entity> entities, GridSystem grid) : base(entities) 
    { 
        _filter = new ContactFilter2D();
        _filter.SetLayerMask(LayerMask.GetMask("Enemy"));
        _filter.useTriggers = true;
    }

    public override void Update(float deltaTime) 
    {
        // 筛选拥有轨迹追踪和物理组件的子弹
        var bullets = GetEntitiesWith<BulletTag, PositionComponent, TraceComponent, PhysicsColliderComponent>();

        foreach (var b in bullets) 
        {
            if (!b.IsAlive) continue;

            var pos = b.GetComponent<PositionComponent>();
            var trace = b.GetComponent<TraceComponent>();
            
            // --- 核心修复：使用射线检测解决穿透 ---
            Vector2 start = new Vector2(trace.PreviousX, trace.PreviousY);
            Vector2 end = new Vector2(pos.X, pos.Y);
            Vector2 direction = end - start;
            float distance = direction.magnitude;

            // 如果位移太小（比如刚生成），退化为重叠检测
            if (distance < 0.01f)
            {
                var bPhys = b.GetComponent<PhysicsColliderComponent>();
                Collider2D[] overlapResults = new Collider2D[1];
                if (bPhys.Collider.OverlapCollider(_filter, overlapResults) > 0)
                {
                    HandleHit(b, overlapResults[0].gameObject);
                }
                continue;
            }

            // 发射射线扫描本帧经过的轨迹
            int hitCount = Physics2D.Raycast(start, direction.normalized, _filter, _results, distance);
            
            if (hitCount > 0)
            {
                // 击中轨迹上的第一个目标
                HandleHit(b, _results[0].collider.gameObject);
            }
        }
    }

    private void HandleHit(Entity bullet, GameObject hitGo)
    {
        Entity enemy = ECSManager.Instance.GetEntityFromGameObject(hitGo);
        if (enemy != null && enemy.IsAlive)
        {
            // 挂载命中事件
            bullet.AddComponent(new BulletHitEventComponent(enemy));
        }
    }
}