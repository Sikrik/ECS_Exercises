using System.Collections.Generic;
using UnityEngine;

public class ECSManager : MonoBehaviour
{
    public static ECSManager Instance { get; private set; }
    public List<Entity> _entities = new List<Entity>();
    private List<SystemBase> _systems = new List<SystemBase>();
    
    public Entity PlayerEntity { get; private set; }
    public int Score { get; set; }
    public GameConfig Config { get; private set; }
    public GridSystem Grid { get; private set; }

    public Dictionary<System.Type, List<Entity>> QueryCache = new Dictionary<System.Type, List<Entity>>();
    private Queue<List<Entity>> _listPool = new Queue<List<Entity>>();

    [Header("必须赋值的预制体")]
    public GameObject PlayerPrefab;

    void Awake()
    {
        Instance = this;
        LoadConfig();
    }

    void Start() => InitGame();

    void InitGame()
    {
        // 1. 初始化网格 (CellSize=3)
        Grid = new GridSystem(_entities, 3.0f);
        
        CreatePlayerEntity();

        // 2. 注册系统流水线
        RegisterSystem(new PlayerInputSystem(_entities));
        RegisterSystem(new EnemyAISystem(_entities));
        RegisterSystem(new MovementSystem(_entities));
        
        RegisterSystem(Grid); // 更新空间索引
        
        // 修复点：确保构造函数参数对齐
        RegisterSystem(new PlayerShootingSystem(_entities, Grid)); 
        RegisterSystem(new BulletCollisionSystem(_entities, Grid));
        RegisterSystem(new BulletEffectSystem(_entities)); 
        RegisterSystem(new HealthSystem(_entities));
        
        // 修复点：EnemySpawnSystem 只需要实体列表
        RegisterSystem(new EnemySpawnSystem(_entities)); 

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

    private void CreatePlayerEntity()
    {
        PlayerEntity = CreateEntity();
        PlayerEntity.AddComponent(new PositionComponent(0, 0, 0));
        PlayerEntity.AddComponent(new VelocityComponent(0, 0, 0));
        PlayerEntity.AddComponent(new PlayerComponent());
        PlayerEntity.AddComponent(new HealthComponent(Config.PlayerMaxHealth));
        PlayerEntity.AddComponent(new CollisionComponent(Config.PlayerCollisionRadius));
        
        if (PlayerPrefab != null)
        {
            GameObject go = Instantiate(PlayerPrefab);
            PlayerEntity.AddComponent(new ViewComponent(go));
        }
    }

    public void DestroyEntity(Entity entity)
    {
        if (entity == null || !entity.IsAlive) return;
        entity.MarkAsDead();

        if (entity.HasComponent<ViewComponent>())
            PoolManager.Instance.Despawn(entity.GetComponent<ViewComponent>().GameObject);

        int index = _entities.IndexOf(entity);
        if (index != -1)
        {
            _entities[index] = _entities[_entities.Count - 1];
            _entities.RemoveAt(_entities.Count - 1);
        }
    }

    public List<Entity> GetListFromPool() => _listPool.Count > 0 ? _listPool.Dequeue() : new List<Entity>();
    public void ReturnListToPool(List<Entity> list) { list.Clear(); _listPool.Enqueue(list); }
    private void RegisterSystem(SystemBase sys) => _systems.Add(sys);
    private void LoadConfig() { Config = new GameConfig(); }
}