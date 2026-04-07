using System;
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
    
    public Dictionary<Type, List<Entity>> QueryCache = new Dictionary<Type, List<Entity>>();

    void Awake()
    {
        Instance = this;
        Config = ConfigLoader.Load();
    }

    void Start()
    {
        PlayerEntity = PlayerFactory.Create(PlayerPrefab, Config);
        _systems = SystemBootstrap.CreateDefaultSystems(_entities, out var grid);
        Grid = grid; 
    }

    void Update()
    {
        // 核心 0 GC 清理：帧首统一归还上一帧缓存的查询列表
        foreach (var list in QueryCache.Values)
        {
            ListPool.Return(list); // 改为调用 ListPool
        }
        QueryCache.Clear();

        float deltaTime = Time.deltaTime;
        for (int i = 0; i < _systems.Count; i++)
        {
            _systems[i].Update(deltaTime);
        }
    }

    /// <summary>
    /// 创建实体：由池化中心代劳
    /// </summary>
    public Entity CreateEntity()
    {
        Entity e = EntityPool.Get();
        _entities.Add(e);
        return e;
    }

    /// <summary>
    /// 移除实体：清理并归还池子
    /// </summary>
    public void RemoveEntityInternal(Entity e)
    {
        if (_entities.Remove(e))
        {
            EntityPool.Return(e);
        }
    }

    // 视图注册映射
    public void UnregisterView(GameObject go) { if (go != null) _gameObjectToEntity.Remove(go.GetInstanceID()); }
    public void RegisterEntityView(GameObject g, Entity e) => _gameObjectToEntity[g.GetInstanceID()] = e;
    public Entity GetEntityFromGameObject(GameObject g) => (g != null && _gameObjectToEntity.TryGetValue(g.GetInstanceID(), out var e)) ? e : null;

    // 快捷访问池的接口
    public List<Entity> GetListFromPool() => ListPool.Get();
    public void ReturnListToPool(List<Entity> l) => ListPool.Return(l);

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}