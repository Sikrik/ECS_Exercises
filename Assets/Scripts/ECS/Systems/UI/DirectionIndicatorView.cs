using UnityEngine;

/// <summary>
/// 挂载在玩家或敌人预制体上，提供箭头引用及其他同步旋转的物体
/// </summary>
public class DirectionIndicatorView : MonoBehaviour
{
    [Tooltip("主方向指示箭头")]
    public Transform ArrowPivot;
    
    [Tooltip("所有需要同步旋转的额外物体（如闪电图标、圆环血条等），拖入这个数组")]
    public Transform[] SyncPivots; 
}