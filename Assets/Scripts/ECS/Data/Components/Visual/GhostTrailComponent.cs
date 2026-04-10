using UnityEngine;

/// <summary>
/// 纯逻辑标记组件：表示该实体当前应该产生残影
/// </summary>
public class GhostTrailComponent : Component
{
    public float SpawnInterval; // 生成残影的时间间隔（秒）
    public float Timer;         // 内部计时器

    public GhostTrailComponent(float interval = 0.04f)
    {
        SpawnInterval = interval;
        Timer = 0;
    }
}