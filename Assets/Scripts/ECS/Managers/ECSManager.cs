using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // 必须引用，用于实现重启功能

public class ECSManager : MonoBehaviour
{
    public static ECSManager Instance;
    
    [Header("配置与预制体")]
    public GameConfig Config;
    public GameObject PlayerPrefab;

    [Header("全局状态")]
    public int Score = 0; // 全局得分

    private List<Entity> _entities = new List<Entity>();
    private List<SystemBase> _systems = new List<SystemBase>();
    
    // 仿 DOTS 核心：GameObject 实例 ID 到 Entity 的快速映射表
    private Dictionary<int, Entity> _gameObjectToEntity = new Dictionary<int, Entity>();

    public Entity PlayerEntity { get; private set; }
    public GridSystem Grid { get; private set; }

    // 用于 SystemBase 查询优化
    public Dictionary<System.Type, List<Entity>> QueryCache = new Dictionary<System.Type, List<Entity>>();
    private Stack<List<Entity>> _listPool = new Stack<List<Entity>>();

    void Awake()
    {
        Instance = this;
        LoadConfig(); // 加载 JSON 配置
    }

    void Start()
    {
        CreatePlayer();
        InitSystems();
    }

    void Update()
    {
        // 1. 每帧开始前清理查询缓存
        foreach (var list in QueryCache.Values) ReturnListToPool(list);
        QueryCache.Clear();

        // 2. 依次运行所有系统
        float deltaTime = Time.deltaTime;
        for (int i = 0; i < _systems.Count; i++)
        {
            _systems[i].Update(deltaTime);
        }
    }

    /// <summary>
    /// 从 Resources 加载游戏配置
    /// </summary>
    private void LoadConfig()
    {
        TextAsset configText = Resources.Load<TextAsset>("game_config");
        if (configText != null)
            Config = JsonUtility.FromJson<GameConfig>(configText.text);
        else
            Config = new GameConfig(); // 容错处理
    }

    /// <summary>
    /// 初始化并排序所有系统
    /// </summary>
    private void InitSystems()
    {
        _systems.Clear();
        
        // 初始化空间网格
        Grid = new GridSystem(2.0f, _entities); 
        
        // --- 核心修复：必须将 Grid 加入更新列表，否则敌人坐标不刷新导致无法射击 ---
        _systems.Add(Grid); 

        // 仿 DOTS 烘焙系统：必须放在最前面处理新生的物理组件
        _systems.Add(new PhysicsBakingSystem(_entities)); 
        
        _systems.Add(new PlayerInputSystem(_entities));
        _systems.Add(new EnemyAISystem(_entities));
        _systems.Add(new EnemySpawnSystem(_entities));
        _systems.Add(new PlayerShootingSystem(_entities, Grid));
        _systems.Add(new MovementSystem(_entities));
        
        // 碰撞系统：放在移动之后处理反弹
        _systems.Add(new CollisionSystem(_entities));     
        _systems.Add(new BulletCollisionSystem(_entities, Grid)); 
        
        _systems.Add(new BulletEffectSystem(_entities));
        _systems.Add(new HealthSystem(_entities));
        _systems.Add(new LifetimeSystem(_entities));
        _systems.Add(new SlowEffectSystem(_entities));
        _systems.Add(new InvincibleVisualSystem(_entities));
        _systems.Add(new ViewSyncSystem(_entities));
    }

    private void CreatePlayer()
    {
        if (PlayerPrefab == null) return;
        GameObject go = Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity);

        PlayerEntity = CreateEntity();
        PlayerEntity.AddComponent(new PlayerTag());
        PlayerEntity.AddComponent(new PositionComponent(0, 0, 0));
        PlayerEntity.AddComponent(new VelocityComponent(0, 0)); 
        PlayerEntity.AddComponent(new HealthComponent(Config.PlayerMaxHealth));
        PlayerEntity.AddComponent(new ViewComponent(go, PlayerPrefab));
        
        // 标记需要物理烘焙
        PlayerEntity.AddComponent(new NeedsBakingTag());
    }

    /// <summary>
    /// 重启游戏逻辑
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); //
    }

    /// <summary>
    /// 创建实体并加入全局列表
    /// </summary>
    public Entity CreateEntity()
    {
        Entity entity = new Entity();
        _entities.Add(entity);
        return entity;
    }

    /// <summary>
    /// 注册 GameObject ID 到 Entity 的映射（供物理查询使用）
    /// </summary>
    public void RegisterEntityView(GameObject go, Entity entity)
    {
        if (go == null) return;
        _gameObjectToEntity[go.GetInstanceID()] = entity;
    }

    /// <summary>
    /// 根据碰撞到的 GameObject 找回 Entity
    /// </summary>
    public Entity GetEntityFromGameObject(GameObject go)
    {
        if (go != null && _gameObjectToEntity.TryGetValue(go.GetInstanceID(), out var entity))
            return entity;
        return null;
    }

    /// <summary>
    /// 统一销毁接口：同步清理物理映射和对象池
    /// </summary>
    public void DestroyEntity(Entity entity)
    {
        if (entity.HasComponent<ViewComponent>())
        {
            var view = entity.GetComponent<ViewComponent>();
            if (view.GameObject != null)
            {
                _gameObjectToEntity.Remove(view.GameObject.GetInstanceID());
                
                if (view.Prefab != null)
                    PoolManager.Instance.Despawn(view.Prefab, view.GameObject);
                else
                    Destroy(view.GameObject);
            }
        }
        entity.IsAlive = false;
        _entities.Remove(entity);
    }

    // --- 性能优化：列表池化 ---
    public List<Entity> GetListFromPool() => _listPool.Count > 0 ? _listPool.Pop() : new List<Entity>();
    public void ReturnListToPool(List<Entity> list) { list.Clear(); _listPool.Push(list); }
}