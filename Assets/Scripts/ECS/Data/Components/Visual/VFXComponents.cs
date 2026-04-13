using UnityEngine;



public class LightningVFXComponent : Component 
{
    public Vector3 StartPos, EndPos;
    public float Duration, Timer;
    public LightningVFXComponent(Vector3 s, Vector3 e, float d = 0.15f) { StartPos = s; EndPos = e; Duration = d; Timer = 0; }
}

public class AttachedVFXComponent : Component 
{
    public GameObject EffectObject; // 挂载在实体上的跟随特效（如冰冻气息）
    public AttachedVFXComponent(GameObject go) => EffectObject = go;
}
public class VFXSpawnEventComponent : Component
{
    public string VFXType;
    public Vector3 Position;
    public Vector3 EndPosition;
    public Entity AttachTarget;
    // 【新增】通用参数，用来传递角度或特效特殊数值
    public float NumericParam = 90f; 
}
// 用于记录实体当前正在显示的冲刺预警红框物体，方便表现层随时更新和回收它
public class ActiveDashPreviewComponent : Component
{
    public UnityEngine.GameObject PreviewObject;
    
    public ActiveDashPreviewComponent(UnityEngine.GameObject go)
    {
        PreviewObject = go;
    }
}