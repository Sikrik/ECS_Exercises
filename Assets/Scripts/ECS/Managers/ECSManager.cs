using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ECSManager : MonoBehaviour
{
    // --- 新增：全局得分 ---
    public int Score = 0;
    public static ECSManager Instance;
    public GameConfig Config;
    public GameObject PlayerPrefab;

    private List<Entity> _entities = new List<Entity>();
    private List<SystemBase> _systems = new List<SystemBase>();
    
    // --- 仿 DOTS 核心：GameObject 到 Entity 的映射表 ---
    private Dictionary<int, Entity> _gameObjectToEntity = new Dictionary<int, Entity>();

    public Entity PlayerEntity { get; private set; }
    public GridSystem Grid { get; private set; }

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
        Grid = new GridSystem(2.0f);
        
        // 注意系统顺序：先烘焙(Baking)，再处理 AI 和 物理检测
        _systems.Add(new PhysicsBakingSystem(_entities)); // 自动化烘焙
        _systems.Add(new PlayerInputSystem(_entities));
        _systems.Add(new EnemyAISystem(_entities));
        _systems.Add(new EnemySpawnSystem(_entities));
        _systems.Add(new PlayerShootingSystem(_entities, Grid));
        _systems.Add(new MovementSystem(_entities));
        _systems.Add(new CollisionSystem(_entities));     // 基于法线的物理反弹
        _systems.Add(new BulletCollisionSystem(_entities));
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
        PlayerEntity.AddComponent(new VelocityComponent(0, 0, 0));
        PlayerEntity.AddComponent(new HealthComponent(Config.PlayerMaxHealth));
        PlayerEntity.AddComponent(new ViewComponent(go, PlayerPrefab));
        
        // --- 触发自动化烘焙 ---
        PlayerEntity.AddComponent(new NeedsBakingTag());
    }

    public Entity CreateEntity()
    {
        Entity entity = new Entity();
        _entities.Add(entity);
        return entity;
    }

    // 映射表注册接口
    public void RegisterEntityView(GameObject go, Entity entity)
    {
        if (go == null) return;
        _gameObjectToEntity[go.GetInstanceID()] = entity;
    }

    // 映射表查询接口
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
                // 销毁实体时同步清理映射表
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
    // --- 新增：重启游戏方法 ---
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}