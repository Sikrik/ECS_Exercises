using UnityEngine;

/// <summary>
/// 方向指示器组件 (通用)
/// </summary>
public class DirectionIndicatorComponent : Component
{
    public Transform ArrowPivot;
    public float RotationSpeed;

    public DirectionIndicatorComponent(Transform arrowPivot, float rotationSpeed = 4f)
    {
        ArrowPivot = arrowPivot;
        RotationSpeed = rotationSpeed;
    }
}