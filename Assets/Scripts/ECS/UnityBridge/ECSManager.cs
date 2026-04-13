using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ECS 核心管理器 (MonoBehaviour 桥接层)
/// 职责：驱动系统更新、管理实体生命周期、维护全局查询缓存。
/// </summary>
public class ECSManager : MonoBehaviour
{
    public static ECSManager Instance;
    
    [Header("战斗配置")]
    [Tooltip("如果未发现局外数据，则默认使用的角色类型")]
    public PlayerClass SelectedCharacter = PlayerClass.Standard;

    public GameConfig Config;
    public GameObject PlayerPrefab;
    
    [Header("实时状态")]
    public int Score = 0; 
    public int CurrentWave = 1;
    public int MaxWave = 1;

    private List<Entity> _entities = new List<Entity>();
    private SystemBootstrap _bootstrap;
    
    // 维护 Unity GameObject 实例 ID 到 ECS Entity 的映射，用于物理反馈寻址
    private Dictionary<int, Entity> _gameObjectToEntity = new Dictionary<int, Entity>();

    public Entity PlayerEntity { get; private set; }
    public GridSystem Grid { get; private set; }
    
    // 全局查询缓存，实现同帧内多个系统共享查询结果，达成 0 GC
    public Dictionary<System.Type, List<Entity>> QueryCache = new Dictionary<System.Type, List<Entity>>();
    private List<List<Entity>> _leasedLists = new List<List<Entity>>();

    void Awake()
    {
        Instance = this;
        
        // 在 Awake 阶段加载所有的 CSV 配置表
        Config = ConfigLoader.Load(); 
    }

    void Start()
    {
        // ==========================================
        // 1. 对接局外数据：获取主菜单选中的角色
        // ==========================================
        if (GameDataManager.Instance != null)
        {
            SelectedCharacter = GameDataManager.Instance.SelectedCharacter;
        }

        // ==========================================
        // 2. 初始化玩家与核心系统
        // ==========================================
        // 传入角色类型给 PlayerFactory 进行数据驱动的装配
        PlayerEntity = PlayerFactory.Create(SelectedCharacter, PlayerPrefab, Config);
        
        // 实例化系统引导程序并初始化全局网格索引
        _bootstrap = new SystemBootstrap(_entities);
        Grid = _bootstrap.Grid; 
    }

    void Update()
    {
        // ==========================================
        // 1. 查询缓存清理 (每帧开始前)
        // ==========================================
        // 帧末统一回收从 ListPool 借出的列表，确保内存复用
        foreach (var list in _leasedLists)
        {
            ListPool.Return(list); 
        }
        _leasedLists.Clear();
        QueryCache.Clear();

        // ==========================================
        // 2. 驱动逻辑更新
        // ==========================================
        if (_bootstrap != null)
        {
            // 按照初始化时定义的 SystemGroup 顺序执行逻辑更新
            _bootstrap.Update(Time.deltaTime);
        }
    }

    // ==========================================
    // 实体管理与内存池接口
    // ==========================================

    /// <summary>
    /// 从列表池中借用一个临时列表，用于系统内部查询。
    /// </summary>
    public List<Entity> GetListFromPool()
    {
        List<Entity> list = ListPool.Get();
        _leasedLists.Add(list); 
        return list;
    }

    /// <summary>
    /// 从实体池中申请一个新的实体对象
    /// </summary>
    public Entity CreateEntity()
    {
        Entity e = EntityPool.Get();
        _entities.Add(e);
        return e;
    }

    /// <summary>
    /// 彻底销毁实体并将其交还给对象池
    /// </summary>
    public void RemoveEntityInternal(Entity e)
    {
        if (_entities.Remove(e))
        {
            EntityPool.Return(e); 
        }
    }

    // ==========================================
    // 视觉表现与逻辑关联
    // ==========================================

    /// <summary>
    /// 注册视图关联：当 GameObject 生成时，建立其 InstanceID 与实体的映射
    /// </summary>
    public void RegisterEntityView(GameObject g, Entity e) => _gameObjectToEntity[g.GetInstanceID()] = e;

    /// <summary>
    /// 注销视图关联
    /// </summary>
    public void UnregisterView(GameObject go) { if (go != null) _gameObjectToEntity.Remove(go.GetInstanceID()); }

    /// <summary>
    /// 根据 GameObject 反查对应的逻辑实体
    /// </summary>
    public Entity GetEntityFromGameObject(GameObject g) => (g != null && _gameObjectToEntity.TryGetValue(g.GetInstanceID(), out var e)) ? e : null;

    /// <summary>
    /// 返回主菜单（替代原有的重启当前关卡逻辑）
    /// </summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1;
        // 如果你的主菜单场景不叫 "MainMenu" 而是其他名字，请修改这里的字符串
        // 或者如果主菜单在 Build Settings 里排第0个，也可以用 SceneManager.LoadScene(0);
        SceneManager.LoadScene("MainMenu"); 
    }
}