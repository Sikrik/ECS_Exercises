using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

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
    public GameObject SlowVFXPrefab;        // 减速冰冻特效
    public GameObject LightningChainVFX;   // 闪电链线条特效
    public GameObject ExplosionVFXPrefab;   // 爆炸特效

    void Awake()
    {
        Instance = this;
    }

    public GameObject GetBulletPrefab(BulletType type)
    {
        return type switch
        {
            BulletType.Slow => SlowBulletPrefab,
            BulletType.ChainLightning => ChainLightningBulletPrefab,
            BulletType.AOE => AOEBulletPrefab,
            _ => NormalBulletPrefab
        };
    }

    public GameObject GetEnemyPrefab(EnemyType type)
    {
        return type switch
        {
            EnemyType.Fast => FastEnemyPrefab,
            EnemyType.Tank => TankEnemyPrefab,
            _ => NormalEnemyPrefab
        };
    }

    // 基础生成与回收逻辑（后期可扩展为真正的 Object Pool）
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;
        return Instantiate(prefab, position, rotation);
    }

    public void Despawn(GameObject go)
    {
        if (go != null) Destroy(go);
    }
}