using System.Collections.Generic;
using UnityEngine;

public class GameObject_PoolManager : MonoBehaviour
{
    public static GameObject_PoolManager Instance;

    [Header("Bullet Prefabs")]
    public GameObject NormalBulletPrefab;
    public GameObject SlowBulletPrefab;
    public GameObject ChainLightningBulletPrefab;
    public GameObject AOEBulletPrefab;

    [Header("Enemy Prefabs")]
    public GameObject NormalEnemyPrefab;
    public GameObject FastEnemyPrefab;
    public GameObject TankEnemyPrefab;

    [Header("VFX Prefabs")]
    public GameObject SlowVFXPrefab;
    public GameObject LightningChainVFX;
    public GameObject ExplosionVFXPrefab;

    // 对象池字典：Key 是预制体资源，Value 是处于非激活状态的物体队列
    private Dictionary<GameObject, Queue<GameObject>> _pools = new Dictionary<GameObject, Queue<GameObject>>();

    void Awake() => Instance = this;

    // 根据枚举获取敌人预制体
    public GameObject GetEnemyPrefab(EnemyType type) => type switch {
        EnemyType.Fast => FastEnemyPrefab,
        EnemyType.Tank => TankEnemyPrefab,
        _ => NormalEnemyPrefab
    };

    // 根据枚举获取子弹预制体
    public GameObject GetBulletPrefab(BulletType type) => type switch {
        BulletType.Slow => SlowBulletPrefab,
        BulletType.ChainLightning => ChainLightningBulletPrefab,
        BulletType.AOE => AOEBulletPrefab,
        _ => NormalBulletPrefab
    };

    /// <summary>
    /// 从池子中获取对象。如果池子为空或对象已损坏，则生成新的。
    /// </summary>
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        if (!_pools.ContainsKey(prefab)) _pools[prefab] = new Queue<GameObject>();

        // --- 核心修复：防御性检测，跳过已被销毁的对象 ---
        while (_pools[prefab].Count > 0)
        {
            GameObject go = _pools[prefab].Dequeue();
            if (go != null) // 确保物体在内存中依然存在
            {
                go.SetActive(true);
                go.transform.position = position;
                go.transform.rotation = rotation;
                return go;
            }
        }

        // 池子为空则创建新对象
        return Instantiate(prefab, position, rotation);
    }

    /// <summary>
    /// 将对象回收进池子，而不是销毁它
    /// </summary>
    public void Despawn(GameObject prefab, GameObject go)
    {
        if (go == null || prefab == null) return;

        go.SetActive(false); // 仅仅隐藏，不销毁
        
        if (!_pools.ContainsKey(prefab)) _pools[prefab] = new Queue<GameObject>();
        _pools[prefab].Enqueue(go);
    }
}