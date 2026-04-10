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