using System.Collections.Generic;
using UnityEngine;

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

    // --- 基础设施 ---
    public Dictionary<System.Type, List<Entity>> QueryCache = new Dictionary<System.Type, List<Entity>>();
    private Queue<List<Entity>> _listPool = new Queue<List<Entity>>();
    public GridSystem Grid { get; private set; }

    [Header("预制体引用")]
    public GameObject PlayerPrefab;

    void Awake()
    {
        Instance = this;
        LoadConfig();
        Score = 0;
    }

    void Start() => InitGame();

    void InitGame()
    {
        // 1. 初始化空间分割系统 (网格大小设为 3)
        Grid = new GridSystem(_entities, 3.0f);

        // 2. 创建玩家
        CreatePlayerEntity();

        // 3. 注册系统流水线 (注意顺序)
        RegisterSystem(new PlayerInputSystem(_entities));
        RegisterSystem(new EnemyAISystem(_entities));
        RegisterSystem(new MovementSystem(_entities));
        
        RegisterSystem(Grid); // 更新实体在网格中的位置
        
        RegisterSystem(new PlayerShootingSystem(_entities, Grid)); 
        RegisterSystem(new BulletCollisionSystem(_entities, Grid)); // 基于网格的碰撞检测
        RegisterSystem(new BulletEffectSystem(_entities));          // 组件驱动的效果处理
        
        RegisterSystem(new HealthSystem(_entities));
        RegisterSystem(new EnemySpawnSystem(_entities, null));
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
        GameObject go = Instantiate(PlayerPrefab);
        PlayerEntity.AddComponent(new ViewComponent(go));
    }

    /// <summary>
    /// 高性能销毁：使用 Swap-back 算法实现 O(1) 删除
    /// </summary>
    public void DestroyEntity(Entity entity)
    {
        if (entity == null || !entity.IsAlive) return;
        entity.MarkAsDead();

        // 回收 GameObject
        if (entity.HasComponent<ViewComponent>())
        {
            var view = entity.GetComponent<ViewComponent>();
            if (view != null && view.GameObject != null)
                PoolManager.Instance.Despawn(view.GameObject);
        }

        // Swap-back 优化：将末尾实体移到当前位置再删除末尾
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