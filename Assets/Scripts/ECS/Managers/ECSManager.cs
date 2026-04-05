using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ECS 总管理器：负责实体的生命周期、系统调度以及所有框架级基础设施
/// </summary>
public class ECSManager : MonoBehaviour
{
    public static ECSManager Instance { get; private set; }

    // --- 核心列表 ---
    public List<Entity> _entities = new List<Entity>();
    private List<SystemBase> _systems = new List<SystemBase>();

    // --- 全局状态 ---
    public Entity PlayerEntity { get; private set; }
    public int Score { get; set; }
    public GameConfig Config { get; private set; }

    // --- 查询缓存与 List 池 ---
    public Dictionary<System.Type, List<Entity>> QueryCache = new Dictionary<System.Type, List<Entity>>();
    private Queue<List<Entity>> _listPool = new Queue<List<Entity>>();

    // --- 对象池引用 (属性不能加 [Header]) ---
    public ObjectPool NormalBulletPool { get; private set; }
    public ObjectPool SlowBulletPool { get; private set; }
    public ObjectPool ChainLightningBulletPool { get; private set; }
    public ObjectPool AOEBulletPool { get; private set; }
    public ObjectPool NormalEnemyPool { get; private set; }
    public ObjectPool FastEnemyPool { get; private set; }
    public ObjectPool TankEnemyPool { get; private set; }

    [Header("预制体引用")]
    public GameObject PlayerPrefab;
    public GameObject NormalBulletPrefab;
    public GameObject SlowBulletPrefab;
    public GameObject ChainBulletPrefab;
    public GameObject AOEBulletPrefab;
    public GameObject NormalEnemyPrefab;
    public GameObject FastEnemyPrefab;
    public GameObject TankEnemyPrefab;

    [Header("特效引用")]
    public GameObject LightningChainVFX; 
    public GameObject NormalHitVFX;
    public ObjectPool LightningVFXPool { get; private set; } // 新增特效池属性

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadConfig();
        Score = 0;
        InitPools();
    }

    void Start() => InitGame();

    void InitPools()
    {
        NormalBulletPool = new ObjectPool(NormalBulletPrefab, 20, 100);
        SlowBulletPool = new ObjectPool(SlowBulletPrefab, 10, 50);
        ChainLightningBulletPool = new ObjectPool(ChainBulletPrefab, 10, 50);
        AOEBulletPool = new ObjectPool(AOEBulletPrefab, 10, 50);

        NormalEnemyPool = new ObjectPool(NormalEnemyPrefab, 10, 50);
        FastEnemyPool = new ObjectPool(FastEnemyPrefab, 10, 50);
        TankEnemyPool = new ObjectPool(TankEnemyPrefab, 10, 50);
       
        LightningVFXPool = new ObjectPool(LightningChainVFX, 20, 100);
    }

    void InitGame()
    {
        CreatePlayerEntity();

        RegisterSystem(new PlayerInputSystem(_entities));
        RegisterSystem(new PlayerShootingSystem(_entities));
        RegisterSystem(new EnemyAISystem(_entities));
        RegisterSystem(new MovementSystem(_entities));
        
        RegisterSystem(new BulletCollisionSystem(_entities)); 
        RegisterSystem(new BulletEffectSystem(_entities));    
        
        RegisterSystem(new HealthSystem(_entities));
        RegisterSystem(new EnemySpawnSystem(_entities, NormalEnemyPrefab));
        
        RegisterSystem(new LightningRenderSystem(_entities)); 
        RegisterSystem(new ViewSyncSystem(_entities));
        

    }

    void Update()
    {
        foreach (var list in QueryCache.Values) ReturnListToPool(list);
        QueryCache.Clear();

        float dt = Time.deltaTime;
        foreach (var sys in _systems) sys.Update(dt);
    }

    public Entity CreateEntity() { var e = new Entity(); _entities.Add(e); return e; }

    void CreatePlayerEntity()
    {
        PlayerEntity = CreateEntity();
        PlayerEntity.AddComponent(new PositionComponent(0, 0, 0));
        PlayerEntity.AddComponent(new VelocityComponent(0, 0, 0));
        PlayerEntity.AddComponent(new PlayerComponent());
        PlayerEntity.AddComponent(new HealthComponent(Config.PlayerMaxHealth));
        PlayerEntity.AddComponent(new CollisionComponent(Config.PlayerCollisionRadius));
        
        GameObject go = Instantiate(PlayerPrefab);
        PlayerEntity.AddComponent(new ViewComponent(go));
    }

    public void DestroyEntity(Entity entity)
    {
        // ... 前置代码 ...
        if (entity.HasComponent<ViewComponent>())
        {
            var view = entity.GetComponent<ViewComponent>();
            if (view != null && view.GameObject != null)
            {
                if (entity.HasComponent<BulletComponent>())
                    GetBulletPool(entity.GetComponent<BulletComponent>().Type).Release(view.GameObject);
                else if (entity.HasComponent<EnemyComponent>())
                    GetEnemyPool(entity.GetComponent<EnemyComponent>().Type).Release(view.GameObject);
                else if (entity.HasComponent<LightningVFXComponent>())
                    LightningVFXPool.Release(view.GameObject); // 关键修复：归还到正确的特效池
                else
                    Destroy(view.GameObject);
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

    public List<Entity> GetListFromPool() => _listPool.Count > 0 ? _listPool.Dequeue() : new List<Entity>();
    public void ReturnListToPool(List<Entity> list) { list.Clear(); _listPool.Enqueue(list); }
    private void RegisterSystem(SystemBase sys) => _systems.Add(sys);
    private void LoadConfig() { Config = new GameConfig(); }
}