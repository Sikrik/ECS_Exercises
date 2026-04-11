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
    public string VFXType; // "Explosion", "Lightning" 等
    public Vector3 Position;
    
    // ======== 新增缺失的字段 ========
    public Entity AttachTarget;  // 绑定的目标（如减速特效绑定在敌人身上）
    public Vector3 EndPosition;  // 特效结束坐标（用于闪电链等需要两点的特效）
}