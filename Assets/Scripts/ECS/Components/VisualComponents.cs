using UnityEngine;


public class ViewComponent : Component 
{
    public GameObject GameObject;
    public GameObject Prefab; // 用于 PoolManager 回收
    public ViewComponent(GameObject go, GameObject prefab) { GameObject = go; Prefab = prefab; }
}

public class BaseColorComponent : Component 
{
    public Color Value; // 存储物体的原始颜色，用于受击/冰冻效果恢复
    public BaseColorComponent(Color c) => Value = c;
}

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