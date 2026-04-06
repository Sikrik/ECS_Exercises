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

    // --- 阶段 1：空间环境初始化 ---
    // 必须最先运行，为后续射击和追踪系统提供最新的敌人位置网格
    Grid = new GridSystem(2.0f, _entities); 
    _systems.Add(Grid); //

    // --- 阶段 2：感知层 (捕捉玩家意图与 AI 决策) ---
    // 此时只产生“移动意图”，不直接修改物理速度，实现输入与物理的彻底解耦
    _systems.Add(new InputCaptureSystem(_entities));    // 捕捉键盘按键与子弹切换指令
    _systems.Add(new EnemyTrackingSystem(_entities));   // 计算怪物的追踪方向意图

    // --- 阶段 3：状态控制层 (将意图转化为最终速度) ---
    // 在这里综合处理“想去哪”与“是否处于被击退/硬直状态”
    _systems.Add(new PlayerControlSystem(_entities));   // 将玩家移动意图转为 Velocity
    _systems.Add(new StateTimerSystem(_entities));      // 处理击退、受击硬直的倒计时逻辑

    // --- 阶段 4：生产与物理烘焙 ---
    // 生成新实体并将其 Unity 组件“翻译”为 ECS 数据
    _systems.Add(new EnemySpawnSystem(_entities));      // 定时生成不同类型的敌人
    _systems.Add(new PlayerShootingSystem(_entities, Grid)); // 射击逻辑并计算目标
    _systems.Add(new PhysicsBakingSystem(_entities));   // 烘焙 Collider 并注册视图映射

    // --- 阶段 5：位移执行与物理同步 ---
    // 在进行碰撞检测前，必须先更新坐标并同步给 Unity Transform
    _systems.Add(new MovementSystem(_entities));        // 应用速度更新 Position
    _systems.Add(new ViewSyncSystem(_entities));        // 坐标同步：让物理碰撞体跟上逻辑位置

    // --- 阶段 6：通用碰撞响应流水线 (核心解耦逻辑) ---
    // 基于 CollisionEventComponent 的事件驱动模型
    _systems.Add(new PhysicsDetectionSystem(_entities)); // [核心] 发现物理重叠并产生碰撞事件
    _systems.Add(new DamageSystem(_entities));           // 响应事件：处理扣血计算
    _systems.Add(new KnockbackSystem(_entities));        // 响应事件：处理位置修正（防重叠）与速度反弹
    _systems.Add(new BulletEffectSystem(_entities));     // 响应事件：处理 AOE/闪电/减速并销毁子弹

    // --- 阶段 7：状态维持、生命周期与视觉反馈 ---
    _systems.Add(new SlowEffectSystem(_entities));       // 更新减速状态并恢复颜色表现
    _systems.Add(new HealthSystem(_entities));           // 检查血量归零并执行实体回收
    _systems.Add(new LifetimeSystem(_entities));         // 销毁寿命到期的特效或子弹
    _systems.Add(new InvincibleVisualSystem(_entities)); // 处理受击后的 Alpha 闪烁反馈
    _systems.Add(new VFXSystem(_entities));              // 让减速烟雾等特效跟随逻辑坐标
    _systems.Add(new LightningRenderSystem(_entities));  // 绘制连锁闪电的电弧表现

    // --- 阶段 8：帧末清理 ---
    // 最后一环，移除本帧产生的瞬时碰撞事件，防止下一帧重复触发
    _systems.Add(new EventCleanupSystem(_entities));     //
}

    /// <summary>
    /// 销毁实体并清理所有关联资源
    /// </summary>
    // 仅展示核心修改部分，其余保持不变
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
    
        // --- 核心修复：挂载此标记，让 BakingSystem 去注册 View 和 Collider ---
        PlayerEntity.AddComponent(new NeedsBakingTag()); 
    
        PlayerEntity.AddComponent(new CollisionFilterComponent(LayerMask.GetMask("Enemy")));
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