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
        LoadConfig(); // 优先加载 CSV 配置
    }

    void Start()
    {
        CreatePlayer();
        InitSystems();
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

    /// <summary>
    /// 初始化系统流水线：顺序决定了逻辑优先级
    /// </summary>
    private void InitSystems()
    {
        _systems.Clear();
        Grid = new GridSystem(2.0f, _entities); 
        _systems.Add(Grid); // 必须添加，否则索敌失效

        // --- 1. 感知层 (捕捉输入与移动决策) ---
        _systems.Add(new InputCaptureSystem(_entities));    // 读键盘意图
        _systems.Add(new EnemyTrackingSystem(_entities));   // 怪物追踪决策

        // --- 2. 控制层 (意图转换为物理速度) ---
        _systems.Add(new PlayerControlSystem(_entities));   // 输入意图 -> 速度
        _systems.Add(new StateTimerSystem(_entities));      // 击退/硬直计时器

        // --- 3. 基础生产与物理层 ---
        _systems.Add(new EnemySpawnSystem(_entities));      // 敌人生成
        _systems.Add(new PlayerShootingSystem(_entities, Grid)); // 射击逻辑
        _systems.Add(new PhysicsBakingSystem(_entities));   // 物理组件烘焙
        _systems.Add(new MovementSystem(_entities));        // 坐标位移更新
        _systems.Add(new ViewSyncSystem(_entities));        // 同步坐标到 GameObject

        // --- 4. 战斗响应流水线 ---
        _systems.Add(new PhysicsDetectionSystem(_entities)); // 通用碰撞检测
        _systems.Add(new DamageSystem(_entities));           // 处理伤害计算
        _systems.Add(new KnockbackSystem(_entities));        // 处理物理排斥
        _systems.Add(new BulletEffectSystem(_entities));     // 处理子弹特效并销毁

        // --- 5. 状态维持与视觉反馈 (关键修复点) ---
        _systems.Add(new HealthSystem(_entities));           // 检查死亡
        _systems.Add(new InvincibleVisualSystem(_entities)); // 受击闪烁
        _systems.Add(new VFXSystem(_entities));              // 修复：让特效跟随目标
        _systems.Add(new LightningRenderSystem(_entities));  // 修复：渲染闪电链
        _systems.Add(new EventCleanupSystem(_entities));     // 清理本帧碰撞事件
        _systems.Add(new SlowEffectSystem(_entities));
    }

    /// <summary>
    /// 销毁实体并清理所有关联资源
    /// </summary>
    public void DestroyEntity(Entity e)
    {
        // 1. 清理主体视觉对象
        if (e.HasComponent<ViewComponent>())
        {
            var view = e.GetComponent<ViewComponent>();
            if (view.GameObject != null)
            {
                _gameObjectToEntity.Remove(view.GameObject.GetInstanceID());
                if (view.Prefab != null) 
                    PoolManager.Instance.Despawn(view.Prefab, view.GameObject);
                else 
                    Destroy(view.GameObject);
            }
        }

        // 2. 修复：清理挂载在该实体上的 VFX 特效 (如减速烟雾)
        if (e.HasComponent<AttachedVFXComponent>())
        {
            var vfx = e.GetComponent<AttachedVFXComponent>();
            if (vfx.EffectObject != null)
            {
                // 将特效物体回收到池中
                PoolManager.Instance.Despawn(PoolManager.Instance.SlowVFXPrefab, vfx.EffectObject);
            }
        }

        e.IsAlive = false;
        _entities.Remove(e);
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
        // ... 组件挂载
        PlayerEntity.AddComponent(new ViewComponent(go, PlayerPrefab));
    
        // 2. 核心修复：必须手动注册视图，否则碰撞检测找不到玩家实体！
        RegisterEntityView(go, PlayerEntity);
        
        // 设置玩家的碰撞过滤：只撞敌人层
        PlayerEntity.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Enemy")));
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
            if (i == 1 && key.Length > 0 && key[0] == '\uFEFF') key = key.Substring(1);

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