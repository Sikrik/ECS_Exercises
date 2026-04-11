// 路径: Assets/Scripts/ECS/UnityBridge/ECSManager.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ECSManager : MonoBehaviour
{
    public static ECSManager Instance;
    
    [Header("Select Character")]
    [Tooltip("在这里切换当前出战的角色类型！")]
    public PlayerClass SelectedCharacter = PlayerClass.Standard;

    public GameConfig Config;
    public GameObject PlayerPrefab;
    public int Score = 0; 

    private List<Entity> _entities = new List<Entity>();
    private SystemBootstrap _bootstrap;
    private Dictionary<int, Entity> _gameObjectToEntity = new Dictionary<int, Entity>();

    public Entity PlayerEntity { get; private set; }
    public GridSystem Grid { get; private set; }
    
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
        // 传入选中的角色类型给 PlayerFactory
        PlayerEntity = PlayerFactory.Create(SelectedCharacter, PlayerPrefab, Config);
        
        // 实例化系统引导程序并获取全局网格索引
        _bootstrap = new SystemBootstrap(_entities);
        Grid = _bootstrap.Grid; 
    }

    void Update()
    {
        // 帧末统一回收查询列表（实现 0 GC 核心）
        foreach (var list in _leasedLists)
        {
            ListPool.Return(list); 
        }
        _leasedLists.Clear();
        QueryCache.Clear();

        // 驱动所有 ECS 系统运转
        if (_bootstrap != null)
        {
            _bootstrap.Update(Time.deltaTime);
        }
    }

    public List<Entity> GetListFromPool()
    {
        List<Entity> list = ListPool.Get();
        _leasedLists.Add(list); 
        return list;
    }

    public Entity CreateEntity()
    {
        Entity e = EntityPool.Get();
        _entities.Add(e);
        return e;
    }

    public void RemoveEntityInternal(Entity e)
    {
        if (_entities.Remove(e))
        {
            EntityPool.Return(e); 
        }
    }

    // 维持 Unity GameObject 实例 ID 到 ECS Entity 的映射
    public void RegisterEntityView(GameObject g, Entity e) => _gameObjectToEntity[g.GetInstanceID()] = e;
    public void UnregisterView(GameObject go) { if (go != null) _gameObjectToEntity.Remove(go.GetInstanceID()); }
    public Entity GetEntityFromGameObject(GameObject g) => (g != null && _gameObjectToEntity.TryGetValue(g.GetInstanceID(), out var e)) ? e : null;

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}