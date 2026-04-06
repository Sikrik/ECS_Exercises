using System;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    private Dictionary<int, Entity> _gameObjectToEntity = new Dictionary<int, Entity>();

    public Entity PlayerEntity { get; private set; }
    public GridSystem Grid { get; private set; }
    public Dictionary<Type, List<Entity>> QueryCache = new Dictionary<Type, List<Entity>>();
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

    private void LoadConfig()
    {
        TextAsset csvText = Resources.Load<TextAsset>("game_config");
        if (csvText == null) return;

        Config = new GameConfig();
        string[] lines = csvText.text.Split('\n');
        FieldInfo[] fields = typeof(GameConfig).GetFields(BindingFlags.Public | BindingFlags.Instance);

        for (int i = 1; i < lines.Length; i++) 
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            
            string[] columns = line.Split(',');
            if (columns.Length < 2) continue;

            string key = columns[0].Trim();
            if (i == 1 && key.Length > 0 && key[0] == '\uFEFF')
            {
                key = key.Substring(1);
            }

            foreach (var field in fields)
            {
                if (field.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    object val = Convert.ChangeType(columns[1].Trim(), field.FieldType, CultureInfo.InvariantCulture);
                    field.SetValue(Config, val);
                    break;
                }
            }
        }
    }

    private void InitSystems()
    {
        _systems.Clear();
        Grid = new GridSystem(2.0f, _entities); 
        _systems.Add(Grid); // 关键修复：必须添加到系统列表，它才会运行！

        // 1. 输入与AI
        _systems.Add(new PlayerInputSystem(_entities));
        _systems.Add(new EnemyAISystem(_entities));

        // 2. 生产与物理烘焙
        _systems.Add(new EnemySpawnSystem(_entities));
        _systems.Add(new PlayerShootingSystem(_entities, Grid));
        _systems.Add(new PhysicsBakingSystem(_entities)); 

        // 3. 位移与空间同步 (必须在碰撞前运行)
        _systems.Add(new MovementSystem(_entities));
        _systems.Add(new ViewSyncSystem(_entities));

        // 4. 通用碰撞架构 (已摘除旧的专用系统)
        _systems.Add(new PhysicsDetectionSystem(_entities));
        _systems.Add(new DamageSystem(_entities));
        _systems.Add(new KnockbackSystem(_entities));
        _systems.Add(new BulletEffectSystem(_entities));

        // 5. 状态与生命周期
        _systems.Add(new SlowEffectSystem(_entities));
        _systems.Add(new HealthSystem(_entities));
        _systems.Add(new LifetimeSystem(_entities));
        
        // 6. 视觉与清理
        _systems.Add(new LightningRenderSystem(_entities));
        _systems.Add(new VFXSystem(_entities));
        _systems.Add(new InvincibleVisualSystem(_entities));
        _systems.Add(new EventCleanupSystem(_entities));
    }

    private void CreatePlayer()
    {
        if (PlayerPrefab == null) return;
        
        GameObject go = Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity);

        PlayerEntity = CreateEntity();
        PlayerEntity.AddComponent(new PlayerTag());
        PlayerEntity.AddComponent(new PositionComponent(0, 0, 0));
        PlayerEntity.AddComponent(new VelocityComponent(0, 0)); 
        PlayerEntity.AddComponent(new HealthComponent(Config.PlayerMaxHealth));
        PlayerEntity.AddComponent(new ViewComponent(go, PlayerPrefab));
        PlayerEntity.AddComponent(new NeedsBakingTag());
        
        PlayerEntity.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Enemy")));
    }

    public Entity CreateEntity()
    {
        Entity e = new Entity();
        _entities.Add(e);
        return e;
    }

    public void RegisterEntityView(GameObject g, Entity e)
    {
        _gameObjectToEntity[g.GetInstanceID()] = e;
    }

    public Entity GetEntityFromGameObject(GameObject g)
    {
        if (g != null && _gameObjectToEntity.TryGetValue(g.GetInstanceID(), out var e))
        {
            return e;
        }
        return null;
    }

    public void DestroyEntity(Entity e)
    {
        if (e.HasComponent<ViewComponent>())
        {
            var view = e.GetComponent<ViewComponent>();
            if (view.GameObject != null)
            {
                // 必须从映射表中移除，否则物理系统会继续报错
                _gameObjectToEntity.Remove(view.GameObject.GetInstanceID());
            
                // 回收到池子
                if (view.Prefab != null)
                    PoolManager.Instance.Despawn(view.Prefab, view.GameObject);
                else
                    Destroy(view.GameObject);
            }
        }

        e.IsAlive = false;
        _entities.Remove(e);
    }

    public List<Entity> GetListFromPool()
    {
        return _listPool.Count > 0 ? _listPool.Pop() : new List<Entity>();
    }

    public void ReturnListToPool(List<Entity> l)
    {
        l.Clear();
        _listPool.Push(l);
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

