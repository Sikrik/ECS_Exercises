// 路径: Assets/Scripts/ECS/Systems/Visual/AttackPreviewView.cs
using UnityEngine;

/// <summary>
/// 攻击预警视图组件 (挂载在 RangedEnemy 和 ChargerEnemy 的 Prefab 上)
/// 请在 Prefab 上添加 LineRenderer 组件，并拖入此处。
/// 记得把 LineRenderer 的 Material 设为 Sprites-Default，并将 Texture Mode 设为 Tile 或 Stretch。
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class AttackPreviewView : MonoBehaviour
{
    public LineRenderer PreviewLine;

    void Awake()
    {
        if (PreviewLine == null) PreviewLine = GetComponent<LineRenderer>();
        PreviewLine.enabled = false; // 初始隐藏
        PreviewLine.positionCount = 2;
        PreviewLine.sortingOrder = -1; // 渲染在角色下方
    }
}