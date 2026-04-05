using System.Collections.Generic;
using UnityEngine;

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
    public GameObject LightningChainVFX; // 必须带有 LineRenderer
    public GameObject NormalHitVFX;

    // 预制体 -> 对象池 的映射
    private Dictionary<GameObject, ObjectPool> _poolMap = new Dictionary<GameObject, ObjectPool>();
    
    // 实例 -> 对象池 的映射（关键：用于简化回收逻辑）
    private Dictionary<GameObject, ObjectPool> _instanceToPool = new Dictionary<GameObject, ObjectPool>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

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
        
        ObjectPool pool = _poolMap[prefab];
        GameObject go = pool.Get();
        go.transform.SetPositionAndRotation(position, rotation);
        
        // 记录这个实例是由哪个池产生的
        if (!_instanceToPool.ContainsKey(go))
        {
            _instanceToPool.Add(go, pool);
        }

        // 针对闪电链的特殊处理：如果物体没有 LineRenderer 则自动补上
        if (prefab == LightningChainVFX)
        {
            EnsureLineRenderer(go);
        }

        return go;
    }

    /// <summary>
    /// 回收对象：现在的逻辑是“智能回收”，只需要传入实例即可
    /// </summary>
    public void Despawn(GameObject instance)
    {
        if (instance == null) return;

        if (_instanceToPool.TryGetValue(instance, out ObjectPool pool))
        {
            pool.Release(instance);
        }
        else
        {
            // 如果不是从池里生成的，则直接销毁
            Destroy(instance);
        }
    }

    private void EnsureLineRenderer(GameObject go)
    {
        if (go.GetComponent<LineRenderer>() == null)
        {
            var lr = go.AddComponent<LineRenderer>();
            lr.startWidth = lr.endWidth = 0.05f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
        }
    }

    // 辅助方法：根据子弹类型获取预制体
    public GameObject GetBulletPrefab(BulletType type) => type switch {
        BulletType.Slow => SlowBulletPrefab,
        BulletType.ChainLightning => ChainBulletPrefab,
        BulletType.AOE => AOEBulletPrefab,
        _ => NormalBulletPrefab
    };
    
    // 辅助方法：根据敌人类型获取预制体
    public GameObject GetEnemyPrefab(EnemyType type) => type switch {
        EnemyType.Fast => FastEnemyPrefab,
        EnemyType.Tank => TankEnemyPrefab,
        _ => NormalEnemyPrefab
    };
}