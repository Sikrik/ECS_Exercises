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

// 1. 预判走位 AI（适合敏捷型/刺客型敌人，不直接瞄准玩家当前位置，而是预判玩家未来的位置）
public class PredictiveAIComponent : Component
{
    public float PredictTime; // 预判未来的时间（秒）
    public PredictiveAIComponent(float time = 0.5f) => PredictTime = time;
}

// 2. 远程风筝 AI（适合射手，靠近到一定距离后停下开火，太近了会后退）
public class RangedAIComponent : Component
{
    public float PreferredDistance; // 期望保持的射击距离
    public float Tolerance;         // 距离容差，防止频繁前后鬼畜抖动

    public RangedAIComponent(float dist = 6f, float tolerance = 1f)
    {
        PreferredDistance = dist;
        Tolerance = tolerance;
    }
}

// 3. 虫群分离 AI（让怪物主动避开同类，形成包围圈，而不是全挤在一条直线上）
public class SwarmSeparationComponent : Component
{
    public float SeparationWeight; // 排斥力权重
    public SwarmSeparationComponent(float weight = 1.5f) => SeparationWeight = weight;
}