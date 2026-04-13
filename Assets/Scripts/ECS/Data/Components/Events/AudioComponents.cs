using UnityEngine;

/// <summary>
/// 音频播放事件组件 (单帧意图)
/// </summary>
public class AudioPlayEventComponent : Component
{
    public string ClipName;      // 音效的名称/ID
    public bool IsPositional;    // 是否为 3D 空间音效
    public Vector3 Position;     // 如果是空间音效，播放的位置

    public AudioPlayEventComponent(string clipName, bool isPositional = false, Vector3 position = default)
    {
        ClipName = clipName;
        IsPositional = isPositional;
        Position = position;
    }
}