using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用方向指示器系统 (表现层)
/// 职责：监听实体的速度，平滑旋转绑定的箭头指针
/// </summary>
public class DirectionIndicatorSystem : SystemBase
{
    public DirectionIndicatorSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        // 凡是同时拥有“方向指示器”和“物理速度”的实体，统统生效！不管是玩家还是敌人！
        var entities = GetEntitiesWith<DirectionIndicatorComponent, VelocityComponent>();

        foreach (var entity in entities)
        {
            var indicator = entity.GetComponent<DirectionIndicatorComponent>();
            var vel = entity.GetComponent<VelocityComponent>();

            if (indicator.ArrowPivot != null)
            {
                // 只有当实体在移动时，才计算目标方向（静止时保持原样）
                if (vel.VX != 0 || vel.VY != 0)
                {
                    float targetAngle = Mathf.Atan2(vel.VY, vel.VX) * Mathf.Rad2Deg;
                    Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
                    
                    // 平滑旋转
                    indicator.ArrowPivot.localRotation = Quaternion.Slerp(
                        indicator.ArrowPivot.localRotation, 
                        targetRotation, 
                        deltaTime * indicator.RotationSpeed
                    );
                }
            }
        }
        
    }
}