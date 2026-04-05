using System.ComponentModel;
using UnityEngine;

public class LightningVFXComponent : Component
{
    public Vector3 StartPos;
    public Vector3 EndPos;
    public float Duration;      // 总持续时间
    public float Timer;         // 当前计时
    public float JitterAmount;  // 抖动幅度
    public int Segments;        // 闪电折段数
    
    // 缓存 LineRenderer 引用，避免每帧 GetComponent
    public LineRenderer Line;

    public LightningVFXComponent(Vector3 start, Vector3 end, float duration = 0.2f, float jitter = 0.2f, int segments = 5)
    {
        StartPos = start;
        EndPos = end;
        Duration = duration;
        Timer = 0;
        JitterAmount = jitter;
        Segments = segments;
    }
}