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
    public GameObject BossEnemyPrefab; // 【新增】Boss 预制体引用

    public GameObject GetEnemyPrefab(EnemyType type) => type switch {
        EnemyType.Fast => FastEnemyPrefab,
        EnemyType.Tank => TankEnemyPrefab,
        EnemyType.Charger => ChargerEnemyPrefab,
        EnemyType.Ranged => RangedEnemyPrefab, 
        EnemyType.Boss => BossEnemyPrefab, // 【新增】
        _ => NormalEnemyPrefab
    };

    [Header("VFX Prefabs")]
    public GameObject SlowVFXPrefab;
    public GameObject LightningChainVFX;
    public GameObject ExplosionVFXPrefab;
    public GameObject DashPreviewPrefab;
    public GameObject MeleeSlashVFXPrefab;

    private Dictionary<GameObject, Stack<GameObject>> _pools = new Dictionary<GameObject, Stack<GameObject>>();
    private Dictionary<GameObject, Transform> _poolRoots = new Dictionary<GameObject, Transform>();

    void Awake() => Instance = this;
    
    public GameObject GetBulletPrefab(BulletType type) => type switch {
        BulletType.Slow => SlowBulletPrefab,
        BulletType.ChainLightning => ChainLightningBulletPrefab,
        BulletType.AOE => AOEBulletPrefab,
        _ => NormalBulletPrefab
    };

    private Transform GetPoolRoot(GameObject prefab)
    {
        if (!_poolRoots.ContainsKey(prefab))
        {
            GameObject rootGo = new GameObject($"[Pool] {prefab.name}");
            rootGo.transform.SetParent(this.transform); 
            _poolRoots[prefab] = rootGo.transform;
        }
        return _poolRoots[prefab];
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        if (!_pools.ContainsKey(prefab)) _pools[prefab] = new Stack<GameObject>();

        while (_pools[prefab].Count > 0)
        {
            GameObject go = _pools[prefab].Pop(); 
            if (go != null) 
            {
                go.SetActive(true);
                go.transform.SetPositionAndRotation(position, rotation);
                return go;
            }
        }

        return Instantiate(prefab, position, rotation, GetPoolRoot(prefab));
    }

    public void Despawn(GameObject prefab, GameObject go)
    {
        if (go == null || prefab == null) return;
        if (!go.activeSelf) return; 

        go.SetActive(false); 
        
        if (!_pools.ContainsKey(prefab)) _pools[prefab] = new Stack<GameObject>();
        _pools[prefab].Push(go); 
    }
}