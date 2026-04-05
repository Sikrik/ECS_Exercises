using System.Collections.Generic;
using UnityEngine;

public class ECSManager : MonoBehaviour
{
    public static ECSManager Instance { get; private set; }
    public List<Entity> _entities = new List<Entity>();
    public List<SystemBase> _systems = new List<SystemBase>();
    public Entity PlayerEntity { get; private set; }
    public GameConfig Config { get; private set; }
    public int Score { get; set; }

    [Header("预制体")]
    public GameObject PlayerPrefab;
    public GameObject NormalEnemyPrefab;
    public GameObject FastEnemyPrefab;
    public GameObject TankEnemyPrefab;
    public GameObject NormalBulletPrefab;
    public GameObject SlowBulletPrefab;
    public GameObject ChainLightningBulletPrefab;
    public GameObject AOEBulletPrefab;

    [Header("特效")]
    public GameObject NormalHitVFX;
    public GameObject SlowHitVFX;
    public GameObject LightningHitVFX;
    public GameObject ExplosionVFX;
    public GameObject LightningChainVFX;
    public GameObject SlowEffectVFX;

    // 对象池
    public ObjectPool NormalBulletPool { get; private set; }
    public ObjectPool SlowBulletPool { get; private set; }
    public ObjectPool ChainLightningBulletPool { get; private set; }
    public ObjectPool AOEBulletPool { get; private set; }
    public ObjectPool NormalEnemyPool { get; private set; }
    public ObjectPool FastEnemyPool { get; private set; }
    public ObjectPool TankEnemyPool { get; private set; }

    public Dictionary<System.Type, List<Entity>> QueryCache = new Dictionary<System.Type, List<Entity>>();
    private Queue<List<Entity>> _listPool = new Queue<List<Entity>>();

    private Camera _cachedCamera;
    private Vector3 _cachedCameraPos;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        LoadConfig();
        if (Camera.main != null) { _cachedCamera = Camera.main; _cachedCameraPos = _cachedCamera.transform.position; }
    }

    void Start() => InitGame();

    void LoadConfig()
    {
        Config = new GameConfig();
        TextAsset json = Resources.Load<TextAsset>("game_config");
        if (json != null) JsonUtility.FromJsonOverwrite(json.text, Config);
    }

    void Update()
    {
        // 1. 先回收并清除旧缓存
        foreach (var list in QueryCache.Values)
        {
            if (list != null) ReturnListToPool(list);
        }
        QueryCache.Clear();
    
        float deltaTime = Time.deltaTime;
        // 2. 执行系统逻辑
        foreach (var system in _systems)
        {
            system.Update(deltaTime);
        }
    }

    void InitGame()
    {
        // 初始化各对象池
        NormalEnemyPool = new ObjectPool(NormalEnemyPrefab, Config.EnemyPoolInitialSize, Config.EnemyPoolMaxSize);
        FastEnemyPool = new ObjectPool(FastEnemyPrefab, Config.EnemyPoolInitialSize, Config.EnemyPoolMaxSize);
        TankEnemyPool = new ObjectPool(TankEnemyPrefab, Config.EnemyPoolInitialSize, Config.EnemyPoolMaxSize);
        NormalBulletPool = new ObjectPool(NormalBulletPrefab, Config.BulletPoolInitialSize, Config.BulletPoolMaxSize);
        SlowBulletPool = new ObjectPool(SlowBulletPrefab, Config.BulletPoolInitialSize, Config.BulletPoolMaxSize);
        ChainLightningBulletPool = new ObjectPool(ChainLightningBulletPrefab, Config.BulletPoolInitialSize, Config.BulletPoolMaxSize);
        AOEBulletPool = new ObjectPool(AOEBulletPrefab, Config.BulletPoolInitialSize, Config.BulletPoolMaxSize);

        CreatePlayerEntity();

        // 注册系统：严格的执行顺序确保位置更新后再进行碰撞检测
        RegisterSystem(new PlayerInputSystem(_entities));
        RegisterSystem(new EnemyAISystem(_entities));
        RegisterSystem(new PlayerShootingSystem(_entities));
        RegisterSystem(new SlowEffectSystem(_entities));
        RegisterSystem(new MovementSystem(_entities));
        RegisterSystem(new BulletCollisionSystem(_entities)); // 移动后立即检测碰撞
        RegisterSystem(new CollisionSystem(_entities));
        RegisterSystem(new BulletLifeTimeSystem(_entities));
        RegisterSystem(new HealthSystem(_entities));
        RegisterSystem(new EnemySpawnSystem(_entities, NormalEnemyPrefab));
        RegisterSystem(new ViewSyncSystem(_entities));
    }

    void CreatePlayerEntity()
    {
        PlayerEntity = CreateEntity();
        PlayerEntity.AddComponent(new PositionComponent(0, 0, 0));
        PlayerEntity.AddComponent(new VelocityComponent(0, 0, 0));
        PlayerEntity.AddComponent(new PlayerComponent());
        PlayerEntity.AddComponent(new HealthComponent(Config.PlayerMaxHealth));
        PlayerEntity.AddComponent(new CollisionComponent(Config.PlayerCollisionRadius));
        GameObject go = Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity);
        PlayerEntity.AddComponent(new ViewComponent(go));
    }

    public Entity CreateEntity() { Entity e = new Entity(); _entities.Add(e); return e; }

    public void RegisterSystem(SystemBase s) => _systems.Add(s);

    public List<Entity> GetListFromPool() => _listPool.Count > 0 ? _listPool.Dequeue() : new List<Entity>();

    public void ReturnListToPool(List<Entity> l) { l.Clear(); _listPool.Enqueue(l); }

    /// <summary>
    /// 统一销毁方法：标记死亡并安全归还对象池
    /// </summary>
    public void DestroyEntity(Entity entity)
    {
        if (entity == null || !entity.IsAlive) return; // 避免重复处理

        entity.MarkAsDead(); // 核心：立即标记为死亡，防止同帧内其他系统再次处理

        if (entity.HasComponent<ViewComponent>())
        {
            ViewComponent view = entity.GetComponent<ViewComponent>();
            if (view != null && view.GameObject != null)
            {
                if (entity.HasComponent<BulletComponent>())
                {
                    BulletType type = entity.GetComponent<BulletComponent>().Type;
                    GetBulletPool(type).Release(view.GameObject);
                }
                else if (entity.HasComponent<EnemyComponent>())
                {
                    EnemyType type = entity.GetComponent<EnemyComponent>().Type;
                    GetEnemyPool(type).Release(view.GameObject);
                }
                else Object.Destroy(view.GameObject);
            }
        }
        _entities.Remove(entity);
    }

    private ObjectPool GetBulletPool(BulletType t) => t switch {
        BulletType.Slow => SlowBulletPool,
        BulletType.ChainLightning => ChainLightningBulletPool,
        BulletType.AOE => AOEBulletPool,
        _ => NormalBulletPool
    };

    private ObjectPool GetEnemyPool(EnemyType t) => t switch {
        EnemyType.Fast => FastEnemyPool,
        EnemyType.Tank => TankEnemyPool,
        _ => NormalEnemyPool
    };

    public void RestartGame()
    {
        Time.timeScale = 1;
        var temp = new List<Entity>(_entities);
        foreach (var e in temp) DestroyEntity(e);
        _entities.Clear();
        _systems.Clear();
        Score = 0;
        if (_cachedCamera != null) _cachedCamera.transform.position = _cachedCameraPos;
        InitGame();
        UIManager.Instance.GameOverPanel.SetActive(false);
    }
}