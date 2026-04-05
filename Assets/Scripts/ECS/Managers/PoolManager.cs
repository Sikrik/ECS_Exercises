using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    [Header("Bullet Prefabs")]
    public GameObject NormalBulletPrefab, SlowBulletPrefab, ChainLightningBulletPrefab, AOEBulletPrefab;
    [Header("Enemy Prefabs")]
    public GameObject NormalEnemyPrefab, FastEnemyPrefab, TankEnemyPrefab;
    [Header("VFX Prefabs")]
    public GameObject SlowVFXPrefab, LightningChainVFX, ExplosionVFXPrefab;

    private Dictionary<GameObject, Queue<GameObject>> _pools = new Dictionary<GameObject, Queue<GameObject>>();

    void Awake() => Instance = this;

    // 修复符号：获取敌人预制体
    public GameObject GetEnemyPrefab(EnemyType type) => type switch {
        EnemyType.Fast => FastEnemyPrefab,
        EnemyType.Tank => TankEnemyPrefab,
        _ => NormalEnemyPrefab
    };

    // 修复符号：获取子弹预制体
    public GameObject GetBulletPrefab(BulletType type) => type switch {
        BulletType.Slow => SlowBulletPrefab,
        BulletType.ChainLightning => ChainLightningBulletPrefab,
        BulletType.AOE => AOEBulletPrefab,
        _ => NormalBulletPrefab
    };

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation) {
        if (prefab == null) return null;
        if (!_pools.ContainsKey(prefab)) _pools[prefab] = new Queue<GameObject>();

        if (_pools[prefab].Count > 0) {
            GameObject go = _pools[prefab].Dequeue();
            go.SetActive(true);
            go.transform.position = position;
            go.transform.rotation = rotation;
            return go;
        }
        return Instantiate(prefab, position, rotation);
    }

    public void Despawn(GameObject prefab, GameObject go) {
        if (go == null || prefab == null) return;
        go.SetActive(false);
        if (!_pools.ContainsKey(prefab)) _pools[prefab] = new Queue<GameObject>();
        _pools[prefab].Enqueue(go);
    }
}