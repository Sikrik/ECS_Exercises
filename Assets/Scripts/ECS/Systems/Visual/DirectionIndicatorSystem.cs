// 路径: Assets/Scripts/ECS/Systems/Visual/DirectionIndicatorSystem.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用方向指示器系统 (表现层)
/// 职责：监听实体的速度，平滑旋转绑定的箭头指针以及其他同步物体
/// </summary>
public class DirectionIndicatorSystem : SystemBase
{
    public DirectionIndicatorSystem(List<Entity> entities) : base(entities) { }

    public override void Update(float deltaTime)
    {
        var entities = GetEntitiesWith<DirectionIndicatorComponent, VelocityComponent>();

        foreach (var entity in entities)
        {
            var indicator = entity.GetComponent<DirectionIndicatorComponent>();
            var vel = entity.GetComponent<VelocityComponent>();

            // 【核心修复】：增加 0.1f 的死区阈值。
            // 当速度极小（快要停下）时不再更新角度，防止浮点精度导致 UI 疯狂乱转
            if (Mathf.Abs(vel.VX) > 0.1f || Mathf.Abs(vel.VY) > 0.1f)
            {
                float targetAngle = Mathf.Atan2(vel.VY, vel.VX) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
                
                // 1. 主箭头：较高的响应速度
                if (indicator.ArrowPivot != null)
                {
                    indicator.ArrowPivot.localRotation = Quaternion.Slerp(
                        indicator.ArrowPivot.localRotation, 
                        targetRotation, 
                        deltaTime * indicator.RotationSpeed
                    );
                }

                // 2. 额外UI物体：使用较慢的速度，制造浮游延迟感
                if (indicator.SyncPivots != null)
                {
                    foreach (var pivot in indicator.SyncPivots)
                    {
                        if (pivot != null)
                        {
                            pivot.localRotation = Quaternion.Slerp(
                                pivot.localRotation, 
                                targetRotation, 
                                deltaTime * indicator.SyncRotationSpeed 
                            );
                        }
                    }
                }
            }
        }
    }
}