

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ECSManager : MonoBehaviour
{
    public static ECSManager Instance;
    
    public GameConfig Config;
    public GameObject PlayerPrefab;
    public int Score = 0; 

    private List<Entity> _entities = new List<Entity>();
    private List<SystemBase> _systems = new List<SystemBase>();
    
    private Dictionary<int, Entity> _gameObjectToEntity = new Dictionary<int, Entity>();

    public Entity PlayerEntity { get; private set; }
    public GridSystem Grid { get; private set; }
    
    // 查询缓存与租赁追踪
    public Dictionary<System.Type, List<Entity>> QueryCache = new Dictionary<System.Type, List<Entity>>();
    private List<List<Entity>> _leasedLists = new List<List<Entity>>();

    void Awake()
    {
        Instance = this;
        Config = ConfigLoader.Load(); // 加载配置数据
    }

    void Start()
    {
        // 初始化玩家和系统
        PlayerEntity = PlayerFactory.Create(PlayerPrefab, Config);
        _systems = SystemBootstrap.CreateDefaultSystems(_entities, out var grid);
        Grid = grid; 
    }

    void Update()
    {
        // 【核心修复】：0 GC 清理逻辑
        // 1. 归还所有租赁出去的列表到池子中，防止 ListPool 被抽干
        foreach (var list in _leasedLists)
        {
            ListPool.Return(list); 
        }
        _leasedLists.Clear();
        
        // 2. 清空本帧的查询缓存
        QueryCache.Clear();

        // 执行所有系统逻辑
        float deltaTime = Time.deltaTime;
        for (int i = 0; i < _systems.Count; i++)
        {
            _systems[i].Update(deltaTime);
        }
    }

    /// <summary>
    /// 从池中借用列表并登记，确保帧末自动回收
    /// </summary>
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
            EntityPool.Return(e); // 归还实体到池
        }
    }

    public void RegisterEntityView(GameObject g, Entity e) => _gameObjectToEntity[g.GetInstanceID()] = e;
    public void UnregisterView(GameObject go) { if (go != null) _gameObjectToEntity.Remove(go.GetInstanceID()); }
    public Entity GetEntityFromGameObject(GameObject g) => (g != null && _gameObjectToEntity.TryGetValue(g.GetInstanceID(), out var e)) ? e : null;

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}