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
    // 👇 新增：冲锋怪的预制体挂载槽位
    public GameObject ChargerEnemyPrefab; 

    [Header("VFX Prefabs")]
    public GameObject SlowVFXPrefab;
    public GameObject LightningChainVFX;
    public GameObject ExplosionVFXPrefab;
    // 👇 添加这一行：冲刺预警红框的预制体槽位
    public GameObject DashPreviewPrefab;

    // 【核心优化】：将 Queue(FIFO) 替换为 Stack(LIFO)，大幅提升 CPU 高速缓存（Cache）亲和度
    private Dictionary<GameObject, Stack<GameObject>> _pools = new Dictionary<GameObject, Stack<GameObject>>();

    void Awake() => Instance = this;

    // 根据枚举获取敌人预制体
    public GameObject GetEnemyPrefab(EnemyType type) => type switch {
        EnemyType.Fast => FastEnemyPrefab,
        EnemyType.Tank => TankEnemyPrefab,
        EnemyType.Charger => ChargerEnemyPrefab, // 👇 新增：分发冲锋怪预制体
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

        // 池子为空则创建新对象
        return Instantiate(prefab, position, rotation);
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