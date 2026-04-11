using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 挂载在 Player 预制体上，用于向 ECS 暴露玩家专属 UI 元素的引用
/// </summary>
public class PlayerHUDView : MonoBehaviour
{
    public Image HealthRing; // 圆环血条
    public Image FlashIcon;  // 闪电CD图标
    // (ArrowPivot 已被剥离到通用的 DirectionIndicatorView 中)
}