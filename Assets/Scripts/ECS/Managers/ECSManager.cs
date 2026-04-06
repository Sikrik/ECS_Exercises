using System;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ECS 核心管理器：负责环境初始化、配置加载、系统调度及实体生命周期管理
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
    
    // GameObject 实例 ID 到 Entity 的快速映射表（供物理检测回查）
    private Dictionary<int, Entity> _gameObjectToEntity = new Dictionary<int, Entity>();

    public Entity PlayerEntity { get; private set; }
    public GridSystem Grid { get; private set; }

    // 用于 SystemBase 查询优化
    public Dictionary<System.Type, List<Entity>> QueryCache = new Dictionary<System.Type, List<Entity>>();
    private Stack<List<Entity>> _listPool = new Stack<List<Entity>>();

    void Awake()
    {
        Instance = this;
        // 1. 优先加载 Excel 导出的 CSV 配置
        LoadConfig(); 
    }

    void Start()
    {
        // 2. 创建玩家并初始化系统链
        CreatePlayer();
        InitSystems();
    }

    void Update()
    {
        // 每帧开始前清理查询缓存
        foreach (var list in QueryCache.Values) ReturnListToPool(list);
        QueryCache.Clear();

        // 强制同步上一帧的物理变换
        Physics2D.SyncTransforms();

        // 3. 驱动所有系统运行
        float deltaTime = Time.deltaTime;
        for (int i = 0; i < _systems.Count; i++)
        {
            _systems[i].Update(deltaTime);
        }
    }

    /// <summary>
    /// 从 Resources 加载 CSV 游戏配置，利用反射自动匹配字段
    /// </summary>
    private void LoadConfig()
    {
        TextAsset csvText = Resources.Load<TextAsset>("game_config");
    
        if (csvText != null)
        {
            Config = new GameConfig();
            string[] lines = csvText.text.Split('\n');
            FieldInfo[] fields = typeof(GameConfig).GetFields(BindingFlags.Public | BindingFlags.Instance);

            // 跳过第一行表头
            for (int i = 1; i < lines.Length; i++) 
            {
                string line = lines[i].Trim(); 
                if (string.IsNullOrEmpty(line)) continue;

                string[] columns = line.Split(',');
                if (columns.Length < 2) continue;

                string key = columns[0].Trim();
                // 处理 UTF-8 BOM 不可见字符
                if (i == 1 && key.Length > 0 && key[0] == '\uFEFF') key = key.Substring(1);

                string valueStr = columns[1].Trim();

                foreach (var field in fields)
                {
                    if (field.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            // 使用 InvariantCulture 确保跨平台小数点解析正确
                            object convertedValue = Convert.ChangeType(valueStr, field.FieldType, CultureInfo.InvariantCulture);
                            field.SetValue(Config, convertedValue);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"配置字段 {key} 转换失败: {ex.Message}");
                        }
                        break;
                    }
                }
            }
            Debug.Log($"CSV 配置加载成功。玩家血量: {Config.PlayerMaxHealth}");
        }
        else
        {
            Debug.LogError("未找到 Resources/game_config.csv，请检查文件！");
            Config = new GameConfig();
        }
    }

    /// <summary>
    /// 初始化系统链：严格控制更新顺序以保证物理同步与逻辑解耦
    /// </summary>
    private void InitSystems()
    {
        _systems.Clear();

        // --- A. 基础环境与输入 ---
        Grid = new GridSystem(2.0f, _entities); 
        _systems.Add(Grid); 
        _systems.Add(new PlayerInputSystem(_entities));
        _systems.Add(new EnemyAISystem(_entities));

        // --- B. 实体创建与物理烘焙 ---
        _systems.Add(new EnemySpawnSystem(_entities));
        _systems.Add(new PlayerShootingSystem(_entities, Grid));
        // 在生成之后立即烘焙，确保新实体本帧可参与碰撞
        _systems.Add(new PhysicsBakingSystem(_entities)); 

        // --- C. 位移与空间同步 ---
        _systems.Add(new MovementSystem(_entities));
        // 碰撞前必须同步坐标到 GameObject
        _systems.Add(new ViewSyncSystem(_entities));

        // --- D. 通用碰撞与事件处理 (解耦核心) ---
        _systems.Add(new PhysicsDetectionSystem(_entities)); // 仅产生 CollisionEventComponent
        _systems.Add(new DamageSystem(_entities));           // 处理伤害逻辑
        _systems.Add(new BulletEffectSystem(_entities));     // 处理特殊子弹效果
        
        // --- E. 状态维持与生命周期 ---
        _systems.Add(new SlowEffectSystem(_entities));
        _systems.Add(new HealthSystem(_entities));           // 检查死亡
        _systems.Add(new LifetimeSystem(_entities));         // 自动销毁限时物体
        
        // --- F. 视觉反馈与清理 ---
        _systems.Add(new LightningRenderSystem(_entities));
        _systems.Add(new VFXSystem(_entities));
        _systems.Add(new InvincibleVisualSystem(_entities));
        
        // 每帧最后清理掉瞬时的事件组件
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
        PlayerEntity.AddComponent(new NeedsBakingTag()); // 等待 BakingSystem 初始化物理
    }

    public void RestartGame()
    {
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

    // 性能优化：列表池化管理
    public List<Entity> GetListFromPool() => _listPool.Count > 0 ? _listPool.Pop() : new List<Entity>();
    public void ReturnListToPool(List<Entity> list) { list.Clear(); _listPool.Push(list); }
}