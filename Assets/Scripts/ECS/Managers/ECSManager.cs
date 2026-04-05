using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ECS框架的核心管理器，负责实体生命周期、系统调度和游戏状态管理
/// 这是整个ECS架构的大脑，协调所有组件和系统的运行
/// </summary>
public class ECSManager : MonoBehaviour
{
    /// <summary>单例实例，提供全局访问点</summary>
    public static ECSManager Instance { get; private set; }
    
    /// <summary>所有活跃实体的列表，是ECS数据层的核心存储</summary>
    public List<Entity> _entities = new List<Entity>();
    
    /// <summary>系统执行队列，按注册顺序每帧调用Update</summary>
    private List<SystemBase> _systems = new List<SystemBase>();
    
    /// <summary>玩家实体引用，用于快速访问玩家相关数据</summary>
    public Entity PlayerEntity { get; private set; }
    
    /// <summary>玩家得分，用于游戏进度追踪</summary>
    public int Score { get; set; }
    
    /// <summary>游戏配置数据，包含平衡性参数和初始值</summary>
    public GameConfig Config { get; private set; }
    
    /// <summary>空间网格系统，用于优化碰撞检测性能</summary>
    public GridSystem Grid { get; private set; }

    /// <summary>查询缓存，避免每帧重复创建List对象，优化内存分配</summary>
    public Dictionary<System.Type, List<Entity>> QueryCache = new Dictionary<System.Type, List<Entity>>();
    
    /// <summary>列表对象池，复用List实例减少GC压力</summary>
    private Queue<List<Entity>> _listPool = new Queue<List<Entity>>();

    [Header("必须赋值的预制体")]
    /// <summary>玩家预制体，用于创建玩家实体的视觉表现</summary>
    public GameObject PlayerPrefab;

    void Awake()
    {
        Instance = this;
        LoadConfig();
    }

    void Start() => InitGame();

    /// <summary>
    /// 初始化整个游戏世界，包括网格系统、玩家实体和所有处理系统
    /// 此方法决定了游戏的系统架构和执行顺序
    /// </summary>
    void InitGame()
    {
        // 1. 初始化网格 (CellSize=3)
        Grid = new GridSystem(_entities, 3.0f);
        
        CreatePlayerEntity();

        // 2. 注册系统流水线（按执行顺序）
        RegisterSystem(new PlayerInputSystem(_entities));
        RegisterSystem(new EnemyAISystem(_entities));
        RegisterSystem(new MovementSystem(_entities));
        // 关键：注册敌人与玩家的碰撞系统
        RegisterSystem(new CollisionSystem(_entities));
        
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

    /// <summary>
    /// 每帧调用所有注册的系統，驱动整个ECS框架运行
    /// 包含查询缓存清理和列表对象池回收，确保内存效率
    /// </summary>
    void Update()
    {
        // 清理上一帧的查询缓存并回收列表对象
        foreach (var list in QueryCache.Values) ReturnListToPool(list);
        QueryCache.Clear();

        float dt = Time.deltaTime;
        // 按注册顺序执行所有系统
        foreach (var sys in _systems) sys.Update(dt);
    }

    /// <summary>
    /// 创建新实体并添加到实体列表中
    /// 这是游戏中所有动态对象（敌人、子弹等）的创建入口
    /// </summary>
    public Entity CreateEntity() { var e = new Entity(); _entities.Add(e); return e; }

    /// <summary>
    /// 创建并初始化玩家实体，添加所有必需的组件
    /// 玩家是游戏的核心实体，此方法决定了玩家的初始状态和能力
    /// </summary>
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

    /// <summary>
    /// 销毁指定实体，回收其视图资源和内存
    /// 包含对象池优化和列表高效删除算法，保持性能稳定
    /// </summary>
    public void DestroyEntity(Entity entity)
    {
        if (entity == null || !entity.IsAlive) return;
        entity.MarkAsDead();

        if (entity.HasComponent<ViewComponent>())
            PoolManager.Instance.Despawn(entity.GetComponent<ViewComponent>().GameObject);

        // 使用交换删除算法，避免大量元素移动
        int index = _entities.IndexOf(entity);
        if (index != -1)
        {
            _entities[index] = _entities[_entities.Count - 1];
            _entities.RemoveAt(_entities.Count - 1);
        }
    }

    /// <summary>从对象池获取List实例，减少内存分配</summary>
    public List<Entity> GetListFromPool() => _listPool.Count > 0 ? _listPool.Dequeue() : new List<Entity>();
    
    /// <summary>将List实例返回到对象池以供复用</summary>
    public void ReturnListToPool(List<Entity> list) { list.Clear(); _listPool.Enqueue(list); }
    
    /// <summary>注册系统到执行队列中</summary>
    private void RegisterSystem(SystemBase sys) => _systems.Add(sys);
    
    /// <summary>加载游戏配置文件</summary>
    private void LoadConfig()
    {
        Config = new GameConfig(); // 先创建默认配置，防止文件缺失导致报错

        // 尝试从 Assets/Resources/game_config.json 加载
        // 注意：Resources.Load 不需要写后缀名 .json
        TextAsset jsonFile = Resources.Load<TextAsset>("game_config");

        if (jsonFile != null)
        {
            // 将 JSON 字符串内容覆盖到 Config 对象中
            JsonUtility.FromJsonOverwrite(jsonFile.text, Config);
            Debug.Log("游戏配置加载成功！");
        }
        else
        {
            Debug.LogWarning("未找到 game_config.json，将使用默认数值。请确保文件位于 Assets/Resources 目录下。");
        }
    }
    
    /// <summary>
    /// 重新开始游戏
    /// </summary>
    public void RestartGame()
    {
        // 1. 恢复时间缩放（防止在死亡/暂停界面点击时，游戏还是静止的）
        Time.timeScale = 1;

        // 2. 重新加载当前活动的场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
