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

    // --- 全局状态（修复 UI 报错） ---
    public Entity PlayerEntity { get; private set; }
    public int Score { get; set; }
    public GameConfig Config { get; private set; }

    // --- 查询缓存与 List 池（修复 SystemBase 报错） ---
    public Dictionary<System.Type, List<Entity>> QueryCache = new Dictionary<System.Type, List<Entity>>();
    private Queue<List<Entity>> _listPool = new Queue<List<Entity>>();

    // --- 对象池引用（修复 PlayerShootingSystem 报错） ---
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
    public GameObject LightningChainVFX; // 用于渲染闪电的 LineRenderer 预制体
    public GameObject NormalHitVFX;

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
        // 初始化所有对象池
        NormalBulletPool = new ObjectPool(NormalBulletPrefab, 20, 100);
        SlowBulletPool = new ObjectPool(SlowBulletPrefab, 10, 50);
        ChainLightningBulletPool = new ObjectPool(ChainBulletPrefab, 10, 50);
        AOEBulletPool = new ObjectPool(AOEBulletPrefab, 10, 50);

        NormalEnemyPool = new ObjectPool(NormalEnemyPrefab, 10, 50);
        FastEnemyPool = new ObjectPool(FastEnemyPrefab, 10, 50);
        TankEnemyPool = new ObjectPool(TankEnemyPrefab, 10, 50);
    }

    void InitGame()
    {
        CreatePlayerEntity();

        // 注册系统：执行顺序从上到下
        RegisterSystem(new PlayerInputSystem(_entities));
        RegisterSystem(new PlayerShootingSystem(_entities));
        RegisterSystem(new EnemyAISystem(_entities));
        RegisterSystem(new MovementSystem(_entities));
        
        // 核心优化：检测与效果分离
        RegisterSystem(new BulletCollisionSystem(_entities)); // 1. 只负责检测碰撞
        RegisterSystem(new BulletEffectSystem(_entities));    // 2. 只负责处理命中效果
        
        RegisterSystem(new HealthSystem(_entities));
        RegisterSystem(new EnemySpawnSystem(_entities, NormalEnemyPrefab));
        
        RegisterSystem(new LightningRenderSystem(_entities)); // 处理闪电链视觉
        RegisterSystem(new ViewSyncSystem(_entities));
    }

    void Update()
    {
        // 每帧清空缓存并归还 List 到池
        foreach (var list in QueryCache.Values) ReturnListToPool(list);
        QueryCache.Clear();

        float dt = Time.deltaTime;
        foreach (var sys in _systems) sys.Update(dt);
    }

    // --- 实体管理 ---
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
        if (entity == null || !entity.IsAlive) return;
        entity.MarkAsDead();

        if (entity.HasComponent<ViewComponent>())
        {
            var view = entity.GetComponent<ViewComponent>();
            if (view != null && view.GameObject != null)
            {
                // 优化：根据实体携带的组件判断归还到哪个池
                if (entity.HasComponent<BulletComponent>())
                {
                    var type = entity.GetComponent<BulletComponent>().Type;
                    GetBulletPool(type).Release(view.GameObject);
                }
                else if (entity.HasComponent<EnemyComponent>())
                {
                    var type = entity.GetComponent<EnemyComponent>().Type;
                    GetEnemyPool(type).Release(view.GameObject);
                }
                else
                {
                    Destroy(view.GameObject);
                }
            }
        }
        _entities.Remove(entity);
    }

    // --- 辅助方法：匹配对象池 ---
    
    private ObjectPool GetBulletPool(BulletType type) => type switch {
        BulletType.Slow => SlowBulletPool,
        BulletType.ChainLightning => ChainLightningBulletPool,
        BulletType.AOE => AOEBulletPool,
        _ => NormalBulletPool
    };

    // 修复：补全缺失的 GetEnemyPool 方法
    private ObjectPool GetEnemyPool(EnemyType type) => type switch {
        EnemyType.Fast => FastEnemyPool,
        EnemyType.Tank => TankEnemyPool,
        _ => NormalEnemyPool
    };

    // --- 框架基础设施方法 ---
    public List<Entity> GetListFromPool() => _listPool.Count > 0 ? _listPool.Dequeue() : new List<Entity>();
    public void ReturnListToPool(List<Entity> list) { list.Clear(); _listPool.Enqueue(list); }
    private void RegisterSystem(SystemBase sys) => _systems.Add(sys);
    private void LoadConfig() { Config = new GameConfig(); /* 加载逻辑 */ }
}