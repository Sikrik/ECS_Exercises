// 路径: Assets/Scripts/ECS/Systems/Visual/AttackPreviewRenderSystem.cs
using System.Collections.Generic;
using UnityEngine;

public class AttackPreviewRenderSystem : SystemBase
{
    public AttackPreviewRenderSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 筛选出所有拥有预警可视化组件（LineRenderer）和坐标的实体
        var entities = GetEntitiesWith<AttackPreviewVisualComponent, PositionComponent>();

        foreach (var entity in entities)
        {
            var visual = entity.GetComponent<AttackPreviewVisualComponent>();
            var pos = entity.GetComponent<PositionComponent>();

            if (visual.Line == null) continue;

            Vector3 startPos = new Vector3(pos.X, pos.Y, 0);

            // 1. 如果有【瞄准射线意图】(远程怪)
            if (entity.HasComponent<AimLineIntentComponent>())
            {
                var aim = entity.GetComponent<AimLineIntentComponent>();
                visual.Line.enabled = true;
                visual.Line.startWidth = aim.Width;
                visual.Line.endWidth = aim.Width;
                
                // 红色激光质感：起点实心，终点半透明
                visual.Line.startColor = new Color(1f, 0f, 0f, 0.8f);
                visual.Line.endColor = new Color(1f, 0f, 0f, 0.2f);
                
                visual.Line.SetPosition(0, startPos);
                visual.Line.SetPosition(1, startPos + new Vector3(aim.Direction.x, aim.Direction.y, 0) * aim.Length);
            }
            // 2. 如果有【冲刺预警意图】(冲锋怪)
            else if (entity.HasComponent<DashPreviewIntentComponent>())
            {
                var dash = entity.GetComponent<DashPreviewIntentComponent>();
                visual.Line.enabled = true;
                visual.Line.startWidth = dash.Width;
                visual.Line.endWidth = dash.Width;
                
                // 橙色矩形质感：半透明铺满
                visual.Line.startColor = new Color(1f, 0.5f, 0f, 0.4f); 
                visual.Line.endColor = new Color(1f, 0.5f, 0f, 0.4f);
                
                visual.Line.SetPosition(0, startPos);
                visual.Line.SetPosition(1, startPos + new Vector3(dash.Direction.x, dash.Direction.y, 0) * dash.Length);
            }
            // 3. 如果什么意图都没有 (比如蓄力结束，或者被打断了)
            else
            {
                if (visual.Line.enabled)
                {
                    visual.Line.enabled = false; // 关闭显示
                }
            }
        }
    }
}