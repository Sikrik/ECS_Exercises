using UnityEngine;
public class LightningVFXComponent : Component
{
    public Vector3 StartPos;
    public Vector3 EndPos;
    public float Duration;
    public float Timer;
    public LightningVFXComponent(Vector3 start, Vector3 end, float duration = 0.15f)
    {
        StartPos = start; EndPos = end; Duration = duration; Timer = 0;
    }
}