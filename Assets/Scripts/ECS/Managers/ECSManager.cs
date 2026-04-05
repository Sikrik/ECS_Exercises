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
        // 假设 Config 已经通过 Inspector 拖入或在此处通过 Json 加载
        if (Config == null) Config = new GameConfig();
    }

    private void InitSystems()
    {
        _systems.Clear();

        // 1. 输入与空间管理 (基础)
        _systems.Add(new PlayerInputSystem(_entities));
        _systems.Add(new GridSystem(_entities, 2.0f));

        // 2. 生成与决策 (逻辑)
        _systems.Add(new EnemySpawnSystem(_entities));
        _systems.Add(new PlayerShootingSystem(_entities, Grid));
        _systems.Add(new EnemyAISystem(_entities));

        // 3. 物理与冲突处理 (核心)
        _systems.Add(new MovementSystem(_entities));
        _systems.Add(new CollisionSystem(_entities));
        _systems.Add(new BulletCollisionSystem(_entities, Grid));
        _systems.Add(new BulletEffectSystem(_entities));

        // 4. 状态计时与效果 (处理)
        _systems.Add(new SlowEffectSystem(_entities));
        _systems.Add(new LifetimeSystem(_entities));
        _systems.Add(new HealthSystem(_entities));

        // 5. 表现与同步 (视觉)
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

        // 原子化组装玩家实体
        PlayerEntity.AddComponent(new PlayerTag()); // 身份
        PlayerEntity.AddComponent(new PositionComponent(0, 0, 0)); // 位置
        PlayerEntity.AddComponent(new VelocityComponent(0, 0, 0)); // 速度
        PlayerEntity.AddComponent(new HealthComponent(Config.PlayerMaxHealth)); // 血量
        PlayerEntity.AddComponent(new CollisionComponent(Config.PlayerCollisionRadius)); // 碰撞半径
        PlayerEntity.AddComponent(new ViewComponent(go)); // 视图引用
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;
        for (int i = 0; i < _systems.Count; i++)
        {
            _systems[i].Update(deltaTime);
        }
    }

    // 实体管理接口
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
}