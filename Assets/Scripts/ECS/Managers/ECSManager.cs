using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // 记得添加命名空间

public class ECSManager : MonoBehaviour
{
    public static ECSManager Instance;
    public GameConfig Config;
    public GameObject PlayerPrefab;
    
    private List<Entity> _entities = new List<Entity>();
    private List<SystemBase> _systems = new List<SystemBase>();
    public Entity PlayerEntity { get; private set; }
    public int Score = 0;

    // 修复符号：提供 Grid 访问器
    public GridSystem Grid => GetSystem<GridSystem>();

    public Dictionary<System.Type, List<Entity>> QueryCache = new Dictionary<System.Type, List<Entity>>();
    private Stack<List<Entity>> _listPool = new Stack<List<Entity>>();

    void Awake()
    {
        Instance = this;
        LoadConfig(); // 步骤 A：先加载配置
    }

    void Start()
    {
        CreatePlayer(); 
        InitSystems();
    }
    private void LoadConfig()
    {
        // 从 Resources 文件夹加载 json 文件
        TextAsset configText = Resources.Load<TextAsset>("game_config");
        if (configText != null)
        {
            // 将 JSON 字符串转换为 GameConfig 对象
            Config = JsonUtility.FromJson<GameConfig>(configText.text);
            Debug.Log("游戏配置加载成功！");
        }
        else
        {
            Debug.LogError("未找到 game_config.json 文件，请检查 Assets/Resources 目录！");
            // 可以在这里初始化一套默认值，防止空指针
            Config = new GameConfig(); 
        }
    }

    private void InitSystems() {
        _systems.Clear();
        _systems.Add(new PlayerInputSystem(_entities));
        _systems.Add(new GridSystem(_entities, 2.0f));
        _systems.Add(new EnemySpawnSystem(_entities));
        _systems.Add(new PlayerShootingSystem(_entities, Grid));
        _systems.Add(new EnemyAISystem(_entities));
        _systems.Add(new MovementSystem(_entities));
        _systems.Add(new CollisionSystem(_entities));
        _systems.Add(new BulletCollisionSystem(_entities, Grid));
        _systems.Add(new BulletEffectSystem(_entities));
        _systems.Add(new SlowEffectSystem(_entities));
        _systems.Add(new LifetimeSystem(_entities));
        _systems.Add(new HealthSystem(_entities));
        _systems.Add(new VFXSystem(_entities));
        _systems.Add(new LightningRenderSystem(_entities));
        _systems.Add(new InvincibleVisualSystem(_entities));
        _systems.Add(new ViewSyncSystem(_entities));
    }

    private void CreatePlayer() {
        if (PlayerPrefab == null) return;
        GameObject go = Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity);
        PlayerEntity = CreateEntity();
        PlayerEntity.AddComponent(new PlayerTag());
        PlayerEntity.AddComponent(new PositionComponent(0, 0, 0));
        PlayerEntity.AddComponent(new VelocityComponent(0, 0, 0));
        PlayerEntity.AddComponent(new HealthComponent(Config.PlayerMaxHealth));
        PlayerEntity.AddComponent(new CollisionComponent(Config.PlayerCollisionRadius));
        PlayerEntity.AddComponent(new ViewComponent(go, PlayerPrefab)); 
    }

    // 修复符号：添加重新开始逻辑
    public void RestartGame() {
        Time.timeScale = 1;
        // 倒序销毁所有实体
        for (int i = _entities.Count - 1; i >= 0; i--) {
            DestroyEntity(_entities[i]);
        }
        _entities.Clear();
        Score = 0;
        InitSystems();
        CreatePlayer();
    }

    void Update() {
        foreach (var list in QueryCache.Values) ReturnListToPool(list);
        QueryCache.Clear();

        float dt = Time.deltaTime;
        for (int i = 0; i < _systems.Count; i++) _systems[i].Update(dt);
    }

    public List<Entity> GetListFromPool() => _listPool.Count > 0 ? _listPool.Pop() : new List<Entity>();
    
    // 修复符号：确保命名为 ReturnListToPool 
    public void ReturnListToPool(List<Entity> list) { 
        list.Clear(); 
        _listPool.Push(list); 
    }

    public Entity CreateEntity() {
        Entity entity = new Entity();
        _entities.Add(entity);
        return entity;
    }

    public void DestroyEntity(Entity entity) {
        if (entity.HasComponent<ViewComponent>()) {
            var view = entity.GetComponent<ViewComponent>();
            if (view.GameObject != null && view.Prefab != null) {
                PoolManager.Instance.Despawn(view.Prefab, view.GameObject);
            } else if (view.GameObject != null) {
                Destroy(view.GameObject);
            }
        }
        entity.IsAlive = false;
        _entities.Remove(entity);
    }

    public T GetSystem<T>() where T : SystemBase => (T)_systems.Find(s => s is T);
}