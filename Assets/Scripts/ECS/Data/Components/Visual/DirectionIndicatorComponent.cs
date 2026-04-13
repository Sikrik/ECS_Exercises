using UnityEngine;

/// <summary>
/// 方向指示器组件 (通用)
/// </summary>
public class DirectionIndicatorComponent : Component
{
    public Transform ArrowPivot;
    public Transform[] SyncPivots; 
    public float RotationSpeed;
    // 新增：专门给额外UI物体用的延迟旋转速度
    public float SyncRotationSpeed; 

    // 构造函数新增 syncRotationSpeed 参数，默认给个较低的值 2f
    public DirectionIndicatorComponent(Transform arrowPivot, float rotationSpeed = 4f, Transform[] syncPivots = null, float syncRotationSpeed = 2f)
    {
        ArrowPivot = arrowPivot;
        RotationSpeed = rotationSpeed;
        SyncPivots = syncPivots;
        SyncRotationSpeed = syncRotationSpeed;
    }
}