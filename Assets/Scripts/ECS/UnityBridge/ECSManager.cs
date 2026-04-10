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
    
    // 【修复 1】将 List<SystemBase> 替换为 SystemBootstrap 实例引用
    private SystemBootstrap _bootstrap;
    
    private Dictionary<int, Entity> _gameObjectToEntity = new Dictionary<int, Entity>();

    public Entity PlayerEntity { get; private set; }
    public GridSystem Grid { get; private set; }
    
    public Dictionary<System.Type, List<Entity>> QueryCache = new Dictionary<System.Type, List<Entity>>();
    private List<List<Entity>> _leasedLists = new List<List<Entity>>();

    void Awake()
    {
        Instance = this;
        Config = ConfigLoader.Load(); 
    }

    void Start()
    {
        PlayerEntity = PlayerFactory.Create(PlayerPrefab, Config);
        
        // 【修复 2】实例化 SystemBootstrap，并从中获取 GridSystem 引用
        _bootstrap = new SystemBootstrap(_entities);
        Grid = _bootstrap.Grid; 
    }

    void Update()
    {
        foreach (var list in _leasedLists)
        {
            ListPool.Return(list); 
        }
        _leasedLists.Clear();
        QueryCache.Clear();

        // 【修复 3】调用 bootstrap 内部的系统组更新
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

    public void RegisterEntityView(GameObject g, Entity e) => _gameObjectToEntity[g.GetInstanceID()] = e;
    public void UnregisterView(GameObject go) { if (go != null) _gameObjectToEntity.Remove(go.GetInstanceID()); }
    public Entity GetEntityFromGameObject(GameObject g) => (g != null && _gameObjectToEntity.TryGetValue(g.GetInstanceID(), out var e)) ? e : null;

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}