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
        foreach (var list in QueryCache.Values) ReturnListToPool(list);
        QueryCache.Clear();
        float deltaTime = Time.deltaTime;
        for (int i = 0; i < _systems.Count; i++) _systems[i].Update(deltaTime);
    }

    private void InitSystems()
    {
        _systems.Clear();
        Grid = new GridSystem(2.0f, _entities); 
        _systems.Add(Grid); //

        // --- 1. 感知层 ---
        _systems.Add(new InputCaptureSystem(_entities));    // 读键盘
        _systems.Add(new EnemyTrackingSystem(_entities));   // 怪物追踪意图

        // --- 2. 控制层 (意图转换) ---
        _systems.Add(new PlayerControlSystem(_entities));   // 输入意图 -> 物理速度
        _systems.Add(new StateTimerSystem(_entities));      // 击退/硬直倒计时

        // --- 3. 基础逻辑层 ---
        _systems.Add(new EnemySpawnSystem(_entities));
        _systems.Add(new PlayerShootingSystem(_entities, Grid));
        _systems.Add(new PhysicsBakingSystem(_entities)); 
        _systems.Add(new MovementSystem(_entities));
        _systems.Add(new ViewSyncSystem(_entities));

        // --- 4. 战斗响应流水线 ---
        _systems.Add(new PhysicsDetectionSystem(_entities)); // 物理碰撞检测
        _systems.Add(new DamageSystem(_entities));           // 处理伤害
        _systems.Add(new KnockbackSystem(_entities));        // 处理排斥（内含子弹过滤）
        _systems.Add(new BulletEffectSystem(_entities));     // 处理特效并销毁子弹

        // --- 5. 状态与视觉 ---
        _systems.Add(new HealthSystem(_entities));           // 检查死亡并销毁敌人
        _systems.Add(new InvincibleVisualSystem(_entities));
        _systems.Add(new EventCleanupSystem(_entities));     // 最后清理本帧事件
    }

    public void DestroyEntity(Entity e)
    {
        // 核心修复：必须清理关联的视觉对象，否则模型会残留在原地
        if (e.HasComponent<ViewComponent>())
        {
            var view = e.GetComponent<ViewComponent>();
            if (view.GameObject != null)
            {
                _gameObjectToEntity.Remove(view.GameObject.GetInstanceID());
                if (view.Prefab != null) PoolManager.Instance.Despawn(view.Prefab, view.GameObject);
                else Destroy(view.GameObject);
            }
        }
        e.IsAlive = false;
        _entities.Remove(e);
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

