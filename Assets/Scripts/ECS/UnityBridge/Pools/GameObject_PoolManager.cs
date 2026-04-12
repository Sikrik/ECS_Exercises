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
    public GameObject ChargerEnemyPrefab;
    public GameObject RangedEnemyPrefab; 

    public GameObject GetEnemyPrefab(EnemyType type) => type switch {
        EnemyType.Fast => FastEnemyPrefab,
        EnemyType.Tank => TankEnemyPrefab,
        EnemyType.Charger => ChargerEnemyPrefab,
        EnemyType.Ranged => RangedEnemyPrefab, 
        _ => NormalEnemyPrefab
    };

    [Header("VFX Prefabs")]
    public GameObject SlowVFXPrefab;
    public GameObject LightningChainVFX;
    public GameObject ExplosionVFXPrefab;
    public GameObject DashPreviewPrefab;

    // 将 Queue(FIFO) 替换为 Stack(LIFO)，大幅提升 CPU 高速缓存（Cache）亲和度
    private Dictionary<GameObject, Stack<GameObject>> _pools = new Dictionary<GameObject, Stack<GameObject>>();
    
    // 用于收纳不同预制体的父节点字典，保持 Hierarchy 整洁
    private Dictionary<GameObject, Transform> _poolRoots = new Dictionary<GameObject, Transform>();

    void Awake() => Instance = this;
    
    // 根据枚举获取子弹预制体
    public GameObject GetBulletPrefab(BulletType type) => type switch {
        BulletType.Slow => SlowBulletPrefab,
        BulletType.ChainLightning => ChainLightningBulletPrefab,
        BulletType.AOE => AOEBulletPrefab,
        _ => NormalBulletPrefab
    };

    // 获取或创建对应预制体的专属父节点（文件夹）
    private Transform GetPoolRoot(GameObject prefab)
    {
        if (!_poolRoots.ContainsKey(prefab))
        {
            // 创建一个空的 GameObject 作为文件夹
            GameObject rootGo = new GameObject($"[Pool] {prefab.name}");
            // 将这个文件夹设为当前 PoolManager 的子物体
            rootGo.transform.SetParent(this.transform); 
            _poolRoots[prefab] = rootGo.transform;
        }
        return _poolRoots[prefab];
    }

    /// <summary>
    /// 从池子中获取对象。如果池子为空或对象已损坏，则生成新的。
    /// </summary>
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        if (!_pools.ContainsKey(prefab)) _pools[prefab] = new Stack<GameObject>();

        // --- 防御性检测，跳过已被意外 Destroy 销毁的对象 ---
        while (_pools[prefab].Count > 0)
        {
            GameObject go = _pools[prefab].Pop(); // 使用 Pop 取出最新鲜的内存
            if (go != null) // 确保物体在内存中依然存在
            {
                go.SetActive(true);
                // 推荐使用 SetPositionAndRotation 一次性修改 Transform 提升性能
                go.transform.SetPositionAndRotation(position, rotation);
                return go;
            }
        }

        // 池子为空则创建新对象，并把它的父节点设置到对应的分类文件夹下
        return Instantiate(prefab, position, rotation, GetPoolRoot(prefab));
    }

    /// <summary>
    /// 将对象回收进池子，而不是销毁它
    /// </summary>
    public void Despawn(GameObject prefab, GameObject go)
    {
        if (go == null || prefab == null) return;

        // 【核心防御】：防止同一个 GameObject 被其他系统重复回收导致死循环或逻辑错误
        if (!go.activeSelf) return; 

        go.SetActive(false); // 仅仅隐藏，不销毁
        
        if (!_pools.ContainsKey(prefab)) _pools[prefab] = new Stack<GameObject>();
        _pools[prefab].Push(go); // 使用 Push 压入栈顶
    }
}