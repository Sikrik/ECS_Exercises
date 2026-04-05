using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 资源池管理器：负责所有游戏对象的生成与回收，减少频繁销毁带来的内存碎片
/// </summary>
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    [Header("子弹预制体")]
    public GameObject NormalBulletPrefab;
    public GameObject SlowBulletPrefab;
    public GameObject ChainBulletPrefab;
    public GameObject AOEBulletPrefab;

    [Header("敌人预制体")]
    public GameObject NormalEnemyPrefab;
    public GameObject FastEnemyPrefab;
    public GameObject TankEnemyPrefab;

    [Header("特效预制体")]
    public GameObject LightningChainVFX; // 必须包含 LineRenderer 组件
    public GameObject NormalHitVFX;
    public GameObject ExplosionVFX;

    // 预制体与对应对象池的映射表
    private Dictionary<GameObject, ObjectPool> _poolMap = new Dictionary<GameObject, ObjectPool>();

    void Awake() => Instance = this;

    /// <summary>
    /// 从池中获取或创建一个对象
    /// </summary>
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        if (!_poolMap.ContainsKey(prefab))
        {
            // 默认初始化大小10，最大100
            _poolMap[prefab] = new ObjectPool(prefab, 10, 100);
        }
        
        GameObject go = _poolMap[prefab].Get();
        go.transform.SetPositionAndRotation(position, rotation);
        return go;
    }

    /// <summary>
    /// 将对象归还到对应的池中
    /// </summary>
    public void Despawn(GameObject prefab, GameObject instance)
    {
        if (prefab != null && _poolMap.TryGetValue(prefab, out var pool))
        {
            pool.Release(instance);
        }
        else
        {
            // 如果找不到池，则直接物理销毁
            Destroy(instance);
        }
    }
}