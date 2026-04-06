using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement; // 必须引用，用于实现重启功能

public class ECSManager : MonoBehaviour
{
    public static ECSManager Instance;
    
    [Header("配置与预制体")]
    public GameConfig Config;
    public GameObject PlayerPrefab;

    [Header("全局状态")]
    public int Score = 0; // 全局得分

    private List<Entity> _entities = new List<Entity>();
    private List<SystemBase> _systems = new List<SystemBase>();
    
    // 仿 DOTS 核心：GameObject 实例 ID 到 Entity 的快速映射表
    private Dictionary<int, Entity> _gameObjectToEntity = new Dictionary<int, Entity>();

    public Entity PlayerEntity { get; private set; }
    public GridSystem Grid { get; private set; }

    // 用于 SystemBase 查询优化
    public Dictionary<System.Type, List<Entity>> QueryCache = new Dictionary<System.Type, List<Entity>>();
    private Stack<List<Entity>> _listPool = new Stack<List<Entity>>();

    void Awake()
    {
        Instance = this;
        LoadConfig(); // 加载 JSON 配置
    }

    void Start()
    {
        CreatePlayer();
        InitSystems();
    }

    void Update()
    {
        // 1. 每帧开始前清理查询缓存
        foreach (var list in QueryCache.Values) ReturnListToPool(list);
        QueryCache.Clear();
        Physics2D.SyncTransforms();
        // 2. 依次运行所有系统
        float deltaTime = Time.deltaTime;
        for (int i = 0; i < _systems.Count; i++)
        {
            _systems[i].Update(deltaTime);
        }
    }

    /// <summary>
    /// 从 Resources 加载游戏配置
    /// </summary>
    private void LoadConfig()
    {
        TextAsset csvText = Resources.Load<TextAsset>("game_config");
    
        if (csvText != null)
        {
            Config = new GameConfig();
            // 建议：先按行分割，再清理每行的 \r
            string[] lines = csvText.text.Split('\n');
        
            FieldInfo[] fields = typeof(GameConfig).GetFields(BindingFlags.Public | BindingFlags.Instance);

            // 从 i = 1 开始，明确跳过第一行表头 "Key,Value,Description"
            for (int i = 1; i < lines.Length; i++) 
            {
                string line = lines[i].Trim(); // 清理行尾换行符和空格
                if (string.IsNullOrEmpty(line)) continue;

                string[] columns = line.Split(',');
                if (columns.Length < 2) continue;

                string key = columns[0].Trim();
            
                // 核心修复：处理可能存在的不可见 BOM 字符 (有些 Excel 导出的 UTF-8 会带这个)
                if (i == 1 && key.Length > 0 && key[0] == '\uFEFF') 
                    key = key.Substring(1);

                string valueStr = columns[1].Trim();

                foreach (var field in fields)
                {
                    // 使用 OrdinalIgnoreCase 忽略大小写差异更安全
                    if (field.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            // 核心修复：使用 InvariantCulture 确保 0.2 这种浮点数在任何系统语言下都能正确解析
                            object convertedValue = Convert.ChangeType(valueStr, field.FieldType, System.Globalization.CultureInfo.InvariantCulture);
                            field.SetValue(Config, convertedValue);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"字段 {key} 转换失败: {ex.Message}");
                        }
                        break;
                    }
                }
            }
            Debug.Log($"CSV 配置加载完成。玩家血量: {Config.PlayerMaxHealth}, 移动速度: {Config.PlayerMoveSpeed}");
        }
    }

    /// <summary>
    /// 初始化并排序所有系统
    /// </summary>
    private void InitSystems()
    {
        _systems.Clear();
        Grid = new GridSystem(2.0f, _entities); 
        _systems.Add(Grid); 

        _systems.Add(new PhysicsBakingSystem(_entities)); 
        _systems.Add(new PlayerInputSystem(_entities));
        _systems.Add(new EnemyAISystem(_entities));
        _systems.Add(new EnemySpawnSystem(_entities));
        _systems.Add(new PlayerShootingSystem(_entities, Grid));
        _systems.Add(new MovementSystem(_entities));
    
        _systems.Add(new CollisionSystem(_entities));     
        _systems.Add(new BulletCollisionSystem(_entities, Grid)); 
        _systems.Add(new BulletEffectSystem(_entities));
    
        // --- 补全视觉处理系统 ---
        _systems.Add(new LightningRenderSystem(_entities)); // 处理闪电链绘制
        _systems.Add(new VFXSystem(_entities));             // 处理冰冻等特效跟随
    
        _systems.Add(new SlowEffectSystem(_entities));
        _systems.Add(new LifetimeSystem(_entities));
        _systems.Add(new HealthSystem(_entities));
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
        PlayerEntity.AddComponent(new VelocityComponent(0, 0)); 
        PlayerEntity.AddComponent(new HealthComponent(Config.PlayerMaxHealth));
        PlayerEntity.AddComponent(new ViewComponent(go, PlayerPrefab));
        
        // 标记需要物理烘焙
        PlayerEntity.AddComponent(new NeedsBakingTag());
    }

    /// <summary>
    /// 重启游戏逻辑
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); //
    }

    /// <summary>
    /// 创建实体并加入全局列表
    /// </summary>
    public Entity CreateEntity()
    {
        Entity entity = new Entity();
        _entities.Add(entity);
        return entity;
    }

    /// <summary>
    /// 注册 GameObject ID 到 Entity 的映射（供物理查询使用）
    /// </summary>
    public void RegisterEntityView(GameObject go, Entity entity)
    {
        if (go == null) return;
        _gameObjectToEntity[go.GetInstanceID()] = entity;
    }

    /// <summary>
    /// 根据碰撞到的 GameObject 找回 Entity
    /// </summary>
    public Entity GetEntityFromGameObject(GameObject go)
    {
        if (go != null && _gameObjectToEntity.TryGetValue(go.GetInstanceID(), out var entity))
            return entity;
        return null;
    }

    /// <summary>
    /// 统一销毁接口：同步清理物理映射和对象池
    /// </summary>
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

    // --- 性能优化：列表池化 ---
    public List<Entity> GetListFromPool() => _listPool.Count > 0 ? _listPool.Pop() : new List<Entity>();
    public void ReturnListToPool(List<Entity> list) { list.Clear(); _listPool.Push(list); }
}