using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 资源池管理器：负责所有 GameObject 的生命周期。
/// 独立于 ECS 逻辑，专门处理物理对象的生成与回收。
/// </summary>
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [Header("子弹预制体")]
    public GameObject NormalBulletPrefab;
    public GameObject SlowBulletPrefab;
    public GameObject ChainBulletPrefab;
    public GameObject AOEBulletPrefab;

    [Header("特效预制体")]
    public GameObject LightningChainVFX; // 必须带有 LineRenderer
    public GameObject NormalHitVFX;

    // 内部映射表，根据预制体查找对应的对象池
    private Dictionary<GameObject, ObjectPool> _poolMap = new Dictionary<GameObject, ObjectPool>();

    void Awake() => Instance = this;

    /// <summary>
    /// 生成对象：如果池中没有，则会自动初始化。
    /// </summary>
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        if (!_poolMap.ContainsKey(prefab))
        {
            _poolMap[prefab] = new ObjectPool(prefab, 10, 100);
        }
        
        GameObject go = _poolMap[prefab].Get();
        go.transform.SetPositionAndRotation(position, rotation);
        return go;
    }

    /// <summary>
    /// 回收对象。
    /// </summary>
    public void Despawn(GameObject prefab, GameObject instance)
    {
        if (prefab != null && _poolMap.TryGetValue(prefab, out var pool))
            pool.Release(instance);
        else
            Destroy(instance);
    }
}