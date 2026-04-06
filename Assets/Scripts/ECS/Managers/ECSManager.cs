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
        // --- 核心改动：调用独立的加载器 ---
        Config = ConfigLoader.Load("game_config");
    }

    void Start()
    {
        PlayerEntity = PlayerFactory.Create(PlayerPrefab, Config);
        _systems = SystemBootstrap.CreateDefaultSystems(_entities, out var grid);
        Grid = grid; // 缓存网格引用供工厂使用
    }

    void Update()
    {
        // 每帧开始前清理查询缓存
        foreach (var list in QueryCache.Values)
        {
            ReturnListToPool(list);
        }
        QueryCache.Clear();

        float deltaTime = Time.deltaTime;
        // 驱动所有系统按顺序执行
        for (int i = 0; i < _systems.Count; i++)
        {
            _systems[i].Update(deltaTime);
        }
    }
    // 在 ECSManager.cs 类中添加以下方法

    /// <summary>
    /// 真正的物理移除：从实体列表中彻底删除
    /// </summary>
    public void RemoveEntityInternal(Entity e)
    {
        if (_entities.Contains(e))
        {
            _entities.Remove(e);
        }
    }

    /// <summary>
    /// 注销 GameObject 与 Entity 的映射关系
    /// </summary>
    public void UnregisterView(GameObject go)
    {
        if (go != null)
        {
            _gameObjectToEntity.Remove(go.GetInstanceID());
        }
    }
    

    public void DestroyEntity(Entity e)
    {
        // --- 核心修复：物理和视觉层必须同步清理 ---
        if (e.HasComponent<ViewComponent>())
        {
            var view = e.GetComponent<ViewComponent>();
            if (view.GameObject != null)
            {
                // 如果有碰撞体，也需要从映射表中移除
                var col = view.GameObject.GetComponentInChildren<Collider2D>();
                if (col != null) _gameObjectToEntity.Remove(col.gameObject.GetInstanceID());
                else _gameObjectToEntity.Remove(view.GameObject.GetInstanceID());

                if (view.Prefab != null) PoolManager.Instance.Despawn(view.Prefab, view.GameObject);
                else Destroy(view.GameObject);
            }
        }
    
        e.IsAlive = false;
        _entities.Remove(e);
    }
    

    public Entity CreateEntity()
    {
        Entity e = new Entity();
        _entities.Add(e);
        return e;
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