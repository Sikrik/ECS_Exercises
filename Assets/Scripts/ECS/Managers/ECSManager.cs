using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ECSManager : MonoBehaviour
{
    public static ECSManager Instance;
    public GameConfig Config;
    public GameObject PlayerPrefab;

    public int Score = 0; // 全局得分

    private List<Entity> _entities = new List<Entity>();
    private List<SystemBase> _systems = new List<SystemBase>();
    
    private Dictionary<int, Entity> _gameObjectToEntity = new Dictionary<int, Entity>();

    public Entity PlayerEntity { get; private set; }
    public GridSystem Grid { get; private set; } //

    public Dictionary<System.Type, List<Entity>> QueryCache = new Dictionary<System.Type, List<Entity>>();
    private Stack<List<Entity>> _listPool = new Stack<List<Entity>>();

    void Awake()
    {
        Instance = this;
        LoadConfig();
    }

    void Start()
    {
        CreatePlayer();
        InitSystems();
    }

    void Update()
    {
        // 每帧清理查询缓存
        foreach (var list in QueryCache.Values) ReturnListToPool(list);
        QueryCache.Clear();

        float deltaTime = Time.deltaTime;
        foreach (var system in _systems)
        {
            system.Update(deltaTime);
        }
    }

    private void LoadConfig()
    {
        TextAsset configText = Resources.Load<TextAsset>("game_config");
        if (configText != null)
            Config = JsonUtility.FromJson<GameConfig>(configText.text);
        else
            Config = new GameConfig();
    }

    private void InitSystems()
    {
        _systems.Clear();
        // --- 核心修复：构造函数参数匹配 ---
        Grid = new GridSystem(2.0f, _entities); 
        
        _systems.Add(new PhysicsBakingSystem(_entities));
        _systems.Add(new PlayerInputSystem(_entities));
        _systems.Add(new EnemyAISystem(_entities));
        _systems.Add(new EnemySpawnSystem(_entities));
        _systems.Add(new PlayerShootingSystem(_entities, Grid));
        _systems.Add(new MovementSystem(_entities));
        _systems.Add(new CollisionSystem(_entities));
        _systems.Add(new BulletCollisionSystem(_entities, Grid)); // 补全 Grid 参数
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
        PlayerEntity.AddComponent(new VelocityComponent(0, 0)); // 使用新的双参数构造函数
        PlayerEntity.AddComponent(new HealthComponent(Config.PlayerMaxHealth));
        PlayerEntity.AddComponent(new ViewComponent(go, PlayerPrefab));
        PlayerEntity.AddComponent(new NeedsBakingTag());
    }

    public void RestartGame()
    {
        // 简单直接的重启方案：重新加载当前场景
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public Entity CreateEntity()
    {
        Entity entity = new Entity();
        _entities.Add(entity);
        return entity;
    }

    public void RegisterEntityView(GameObject go, Entity entity)
    {
        if (go == null) return;
        _gameObjectToEntity[go.GetInstanceID()] = entity;
    }

    public Entity GetEntityFromGameObject(GameObject go)
    {
        if (go != null && _gameObjectToEntity.TryGetValue(go.GetInstanceID(), out var entity))
            return entity;
        return null;
    }

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

    public List<Entity> GetListFromPool() => _listPool.Count > 0 ? _listPool.Pop() : new List<Entity>();
    public void ReturnListToPool(List<Entity> list) { list.Clear(); _listPool.Push(list); }
}