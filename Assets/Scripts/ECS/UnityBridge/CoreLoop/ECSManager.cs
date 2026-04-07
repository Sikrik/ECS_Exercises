using System;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ECS 核心管理器：负责系统调度、实体生命周期管理及配置加载
/// </summary>
public class ECSManager : MonoBehaviour
{
    public static ECSManager Instance;
    
    [Header("配置与预制体")]
    public GameConfig Config;
    public GameObject PlayerPrefab;

    [Header("全局状态")]
    public int Score = 0; 

    private List<Entity> _entities = new List<Entity>();
    private List<SystemBase> _systems = new List<SystemBase>();
    
    // 👇 新增：逻辑实体对象池，彻底消灭 new Entity() 产生的垃圾
    private Stack<Entity> _entityPool = new Stack<Entity>();
    
    // 映射表：用于从 Unity 的 GameObject 快速找回 ECS Entity
    private Dictionary<int, Entity> _gameObjectToEntity = new Dictionary<int, Entity>();

    public Entity PlayerEntity { get; private set; }
    public GridSystem Grid { get; private set; }
    
    // 查询缓存与列表池，用于性能优化
    public Dictionary<Type, List<Entity>> QueryCache = new Dictionary<Type, List<Entity>>();
    private Stack<List<Entity>> _listPool = new Stack<List<Entity>>();

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
        foreach (var list in QueryCache.Values)
        {
            ReturnListToPool(list);
        }
        QueryCache.Clear();

        float deltaTime = Time.deltaTime;
        for (int i = 0; i < _systems.Count; i++)
        {
            _systems[i].Update(deltaTime);
        }
    }

    /// <summary>
    /// 获取实体（池化获取）
    /// </summary>
    public Entity CreateEntity()
    {
        // 如果池子里有，就拿出来复用；如果池子空了（比如游戏刚开始），才 new 一个新的
        Entity e = _entityPool.Count > 0 ? _entityPool.Pop() : new Entity();
        e.IsAlive = true;
        _entities.Add(e);
        return e;
    }

    /// <summary>
    /// 真正的物理移除：从实体列表中彻底删除
    /// </summary>
    public void RemoveEntityInternal(Entity e)
    {
        if (_entities.Contains(e))
        {
            e.IsAlive = false;
            e.ClearComponents();
            _entities.Remove(e);
            
            // 防御机制：防止它已经在池子里了还被重复 Push 报错
            if (!_entityPool.Contains(e))
            {
                _entityPool.Push(e);
            }
        }
    }

    public void UnregisterView(GameObject go)
    {
        if (go != null)
        {
            _gameObjectToEntity.Remove(go.GetInstanceID());
        }
    }

    public void RegisterEntityView(GameObject g, Entity e) => _gameObjectToEntity[g.GetInstanceID()] = e;

    public Entity GetEntityFromGameObject(GameObject g)
    {
        if (g != null && _gameObjectToEntity.TryGetValue(g.GetInstanceID(), out var e)) return e;
        return null;
    }

    public List<Entity> GetListFromPool() => _listPool.Count > 0 ? _listPool.Pop() : new List<Entity>();

    public void ReturnListToPool(List<Entity> l) { l.Clear(); _listPool.Push(l); }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}