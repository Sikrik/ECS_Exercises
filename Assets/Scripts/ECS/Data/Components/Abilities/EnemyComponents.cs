using UnityEngine;

/// <summary>
/// 冲锋AI组件：记录触发冲锋的距离等参数
/// </summary>
public class ChargerAIComponent : Component
{
    public float TriggerDistance; // 距离玩家多近时开始冲锋

    public ChargerAIComponent(float triggerDistance = 6f)
    {
        TriggerDistance = triggerDistance;
    }
}
/// <summary>
/// 蓄力状态组件：表示实体正在冲刺前的准备阶段
/// </summary>
public class DashPrepStateComponent : Component
{
    public float Timer;        // 剩余蓄力时间
    public Vector2 TargetDir;  // 锁定的冲刺方向

    public DashPrepStateComponent(float duration, Vector2 dir)
    {
        Timer = duration;
        TargetDir = dir;
    }
}

/// <summary>
/// 范围预览意图：表现层看到此组件后，会生成红框预览
/// </summary>
public class DashPreviewIntentComponent : Component
{
    public Vector2 Direction;
    public float Distance;
    public float Width;

    public DashPreviewIntentComponent(Vector2 dir, float dist, float width)
    {
        Direction = dir;
        Distance = dist;
        Width = width;
    }
}