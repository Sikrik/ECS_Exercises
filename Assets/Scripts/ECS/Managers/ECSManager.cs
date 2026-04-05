using System.Collections.Generic;
using UnityEngine;

public class ECSManager : MonoBehaviour
{
    public static ECSManager Instance;

    [Header("Game Configuration")]
    public GameConfig Config;
    public GameObject PlayerPrefab;

    private List<Entity> _entities = new List<Entity>();
    private List<SystemBase> _systems = new List<SystemBase>();

    public Entity PlayerEntity { get; private set; }
    public int Score = 0;
    public GridSystem Grid => GetSystem<GridSystem>();

    // --- 新增：查询缓存与对象池逻辑，用于适配 SystemBase ---
    public Dictionary<System.Type, List<Entity>> QueryCache = new Dictionary<System.Type, List<Entity>>();
    private Stack<List<Entity>> _listPool = new Stack<List<Entity>>();

    public List<Entity> GetListFromPool()
    {
        if (_listPool.Count > 0) return _listPool.Pop();
        return new List<Entity>();
    }

    public void ReturnListToPool(List<Entity> list)
    {
        list.Clear();
        _listPool.Push(list);
    }
    // --------------------------------------------------

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        LoadConfig();
        CreatePlayer();
        InitSystems();
    }

    private void LoadConfig()
    {
        if (Config == null) Config = new GameConfig();
    }

    private void InitSystems()
    {
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

    private void CreatePlayer()
    {
        if (PlayerPrefab == null) return;
        GameObject go = Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity);
        PlayerEntity = CreateEntity();
        PlayerEntity.AddComponent(new PlayerTag());
        PlayerEntity.AddComponent(new PositionComponent(0, 0, 0));
        PlayerEntity.AddComponent(new VelocityComponent(0, 0, 0));
        PlayerEntity.AddComponent(new HealthComponent(Config.PlayerMaxHealth));
        PlayerEntity.AddComponent(new CollisionComponent(Config.PlayerCollisionRadius));
        PlayerEntity.AddComponent(new ViewComponent(go));
    }

    void Update()
    {
        // --- 核心修复：每帧更新前清空缓存并归还列表到对象池 ---
        foreach (var list in QueryCache.Values)
        {
            ReturnListToPool(list);
        }
        QueryCache.Clear();
        // --------------------------------------------------

        float deltaTime = Time.deltaTime;
        for (int i = 0; i < _systems.Count; i++)
        {
            _systems[i].Update(deltaTime);
        }
    }

    public Entity CreateEntity()
    {
        Entity entity = new Entity();
        _entities.Add(entity);
        return entity;
    }

    public void DestroyEntity(Entity entity)
    {
        if (entity.HasComponent<ViewComponent>())
        {
            var view = entity.GetComponent<ViewComponent>();
            if (view.GameObject != null)
            {
                PoolManager.Instance.Despawn(view.GameObject);
            }
        }
        entity.IsAlive = false;
        _entities.Remove(entity);
    }

    public T GetSystem<T>() where T : SystemBase
    {
        return (T)_systems.Find(s => s is T);
    }
    
    public void RestartGame()
    {
        // 1. 恢复游戏时间（防止在 Game Over 暂停状态下重启）
        Time.timeScale = 1;

        // 2. 清理所有现有实体（必须倒序遍历，因为 DestroyEntity 会修改 List）
        for (int i = _entities.Count - 1; i >= 0; i--)
        {
            DestroyEntity(_entities[i]);
        }
        _entities.Clear();

        // 3. 重置游戏数据
        Score = 0;

        // 4. 重新初始化系统（这会重置所有系统的内部计时器，如刷怪计时）
        InitSystems();

        // 5. 重新创建玩家
        CreatePlayer();
    
        Debug.Log("游戏已重新开始，加油鸭！");
    }
}