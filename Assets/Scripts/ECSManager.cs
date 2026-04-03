// ECSManager.cs 优化版本
// 优化内容：
// 1. 调整系统执行顺序，解决逻辑滞后问题
// 2. 保留原有所有功能与对象池优化
using System.Collections.Generic;
using UnityEngine;


public class ECSManager : MonoBehaviour
{
    // 原有代码（保留）
    public static ECSManager Instance { get; private set; }
    public List<Entity> _entities = new List<Entity>();
    public List<SystemBase> _systems = new List<SystemBase>();
    public Entity PlayerEntity { get; private set; }
    public GameConfig Config { get; private set; }
    
    // 新增：缓存相机的初始状态，重启时恢复
    private Camera _cachedCamera;
    private LayerMask _cachedCullingMask;
    private float _cachedOrthographicSize;
    private Vector3 _cachedCameraPosition;
    
    // 新增：全局得分变量（用于得分系统）
    public int Score { get; set; }
    // 预制体引用（原有逻辑保留）
    public GameObject PlayerPrefab;
    public GameObject EnemyPrefab;
    [Header("敌人预制体")]
    public GameObject NormalEnemyPrefab;
    public GameObject FastEnemyPrefab;
    public GameObject TankEnemyPrefab;
    // 废弃：旧的通用BulletPrefab，已被4种新子弹预制体替代
    public GameObject BulletPrefab;
    // ================================== 新增：子弹与特效预制体 ==================================
    [Header("子弹预制体")]
    public GameObject NormalBulletPrefab;
    public GameObject SlowBulletPrefab;
    public GameObject ChainLightningBulletPrefab;
    public GameObject AOEBulletPrefab;
    [Header("命中特效预制体")]
    public GameObject NormalHitVFX;
    public GameObject SlowHitVFX;
    public GameObject LightningHitVFX;
    public GameObject ExplosionVFX;
    public GameObject LightningChainVFX;
    [Header("持续状态特效预制体")]
    public GameObject SlowEffectVFX;
    // ================================== 修复：对象池实例，为4种子弹创建独立对象池 ==================================
    // 敌人对象池，全局唯一（敌人只有一种，无需拆分）
    public ObjectPool EnemyPool { get; private set; }
    // 4种子弹的独立对象池，避免不同子弹混池导致的显示错误
    public ObjectPool NormalBulletPool { get; private set; }
    public ObjectPool SlowBulletPool { get; private set; }
    public ObjectPool ChainLightningBulletPool { get; private set; }
    public ObjectPool AOEBulletPool { get; private set; }
    // 敌人的独立对象池
    public ObjectPool NormalEnemyPool { get; private set; }
    public ObjectPool FastEnemyPool { get; private set; }
    public ObjectPool TankEnemyPool { get; private set; }
    
    // 新增：查询缓存与对象池，用于优化ECS查询性能，减少GC
    public Dictionary<System.Type, List<Entity>> QueryCache = new Dictionary<System.Type, List<Entity>>();
    private Queue<List<Entity>> _listPool = new Queue<List<Entity>>();
    
    void Start()
    {
        InitGame();
    }
    void Awake()
    {
        // 原有单例、配置加载逻辑（保留）
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        LoadConfig();
        Score = 0;
        
        // 新增：游戏启动时，缓存相机的初始状态，重启时恢复
        Camera[] allCameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in allCameras)
        {
            if (cam != null)
            {
                // 缓存每个相机的初始配置
                _cachedCamera = cam;
                _cachedCullingMask = cam.cullingMask;
                _cachedOrthographicSize = _cachedCamera.orthographicSize;
                _cachedCameraPosition = cam.transform.position;
                break; // 缓存第一个相机的初始状态
            }
        }
    }
    // 加载配置的方法（原有逻辑保留）
    void LoadConfig()
    {
        // 先初始化默认配置，确保任何情况下都有可用的默认值
        Config = new GameConfig();
        
        try
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("game_config");
            if (jsonFile == null || string.IsNullOrEmpty(jsonFile.text))
            {
                Debug.LogWarning("未找到game_config.json配置文件或文件为空，将使用默认游戏配置。");
                return;
            }
            // 反序列化配置，覆盖默认值
            GameConfig loadedConfig = JsonUtility.FromJson<GameConfig>(jsonFile.text);
            if (loadedConfig != null)
            {
                Config = loadedConfig;
                Debug.Log("游戏配置加载成功！已应用自定义配置。");
            }
            else
            {
                Debug.LogWarning("配置文件解析失败，将使用默认游戏配置。");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载配置文件时发生错误：{e.Message}，将使用默认游戏配置。");
        }
    }
    
    // 新增：从对象池获取List，复用对象减少GC
    public List<Entity> GetListFromPool()
    {
        if (_listPool.Count > 0)
        {
            var list = _listPool.Dequeue();
            list.Clear();
            return list;
        }
        return new List<Entity>();
    }
    
    // 新增：将List归还到对象池
    public void ReturnListToPool(List<Entity> list)
    {
        _listPool.Enqueue(list);
    }
    
    void Update()
    {
        // 每帧开始时，将缓存的List归还对象池，并清空查询缓存
        // 这样这一帧的查询会重新生成，同时复用List对象，避免GC
        foreach (var list in QueryCache.Values)
        {
            ReturnListToPool(list);
        }
        QueryCache.Clear();
        
        float deltaTime = Time.deltaTime;
        foreach (var system in _systems)
        {
            system.Update(deltaTime);
        }
    }
    // 初始化游戏的方法（修改：初始化4种子弹的独立对象池）
    void InitGame()
    {
        // 初始化敌人的独立对象池
        NormalEnemyPool = new ObjectPool(
            NormalEnemyPrefab, 
            Config.EnemyPoolInitialSize, 
            Config.EnemyPoolMaxSize
        );
        FastEnemyPool = new ObjectPool(
            FastEnemyPrefab, 
            Config.EnemyPoolInitialSize, 
            Config.EnemyPoolMaxSize
        );
        TankEnemyPool = new ObjectPool(
            TankEnemyPrefab, 
            Config.EnemyPoolInitialSize, 
            Config.EnemyPoolMaxSize
        );
        // ================================== 修复：初始化4种子弹的独立对象池 ==================================
        // 初始化4种子弹的独立对象池，使用各自的预制体
        NormalBulletPool = new ObjectPool(
            NormalBulletPrefab, 
            Config.BulletPoolInitialSize, 
            Config.BulletPoolMaxSize
        );
        SlowBulletPool = new ObjectPool(
            SlowBulletPrefab, 
            Config.BulletPoolInitialSize, 
            Config.BulletPoolMaxSize
        );
        ChainLightningBulletPool = new ObjectPool(
            ChainLightningBulletPrefab, 
            Config.BulletPoolInitialSize, 
            Config.BulletPoolMaxSize
        );
        AOEBulletPool = new ObjectPool(
            AOEBulletPrefab, 
            Config.BulletPoolInitialSize, 
            Config.BulletPoolMaxSize
        );
        
        // 敌人对象池（原有逻辑保留）
        EnemyPool = new ObjectPool(
            EnemyPrefab, 
            Config.EnemyPoolInitialSize, 
            Config.EnemyPoolMaxSize
        );
        CreatePlayerEntity();
        
        // ====================== 优化：调整系统执行顺序，解决逻辑滞后问题 ======================
        // 正确的帧更新流程：输入 → AI → 射击 → 效果 → 移动 → 碰撞 → 生命周期 → 健康 → 生成 → 视图同步
        // 1. 输入处理：优先处理玩家输入，更新玩家速度
        RegisterSystem(new PlayerInputSystem(_entities));
        // 2. AI逻辑：处理敌人的AI行为，更新敌人速度
        RegisterSystem(new EnemyAISystem(_entities));
        // 3. 射击逻辑：处理玩家射击，创建子弹实体
        RegisterSystem(new PlayerShootingSystem(_entities));
        // 4. 效果系统：处理减速等状态效果，更新实体速度
        RegisterSystem(new SlowEffectSystem(_entities));
        // 5. 移动系统：统一更新所有实体的位置，基于上一步更新的速度
        RegisterSystem(new MovementSystem(_entities));
        // 6. 碰撞检测：基于最新的位置，检测碰撞并处理碰撞结果
        RegisterSystem(new BulletCollisionSystem(_entities));
        RegisterSystem(new CollisionSystem(_entities));
        // 7. 生命周期：处理子弹的超时销毁
        RegisterSystem(new BulletLifeTimeSystem(_entities));
        // 8. 健康系统：处理血量变化、死亡逻辑
        RegisterSystem(new HealthSystem(_entities));
        // 9. 敌人生成：生成新的敌人实体
        RegisterSystem(new EnemySpawnSystem(_entities, EnemyPrefab));
        // 10. 视图同步：最后将逻辑层的位置同步到视图层，保证显示最新状态
        RegisterSystem(new ViewSyncSystem(_entities));
    }
    // 创建玩家实体的方法（修改：消除硬编码）
    void CreatePlayerEntity()
    {
        PlayerEntity = CreateEntity();
        
        // 给玩家添加所有需要的组件
        PlayerEntity.AddComponent(new PositionComponent(0, 0, 0));
        PlayerEntity.AddComponent(new VelocityComponent(0, 0, 0));
        PlayerEntity.AddComponent(new PlayerComponent());
        // 玩家血量从配置读取
        PlayerEntity.AddComponent(new HealthComponent(Config.PlayerMaxHealth));
        // 玩家碰撞半径从配置读取，消除硬编码0.5f
        PlayerEntity.AddComponent(new CollisionComponent(Config.PlayerCollisionRadius));
        // 创建玩家视图（原有逻辑保留）
        GameObject playerGo = Object.Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity);
        PlayerEntity.AddComponent(new ViewComponent(playerGo));
    }
    // 注册系统的方法（原有逻辑保留）
    public void RegisterSystem(SystemBase system)
    {
        _systems.Add(system);
    }
    // 创建实体的方法（原有逻辑保留）
    public Entity CreateEntity()
    {
        Entity entity = new Entity();
        _entities.Add(entity);
        return entity;
    }
    // RestartGame 
    // ================================== 修改：RestartGame，完全重置相机的所有初始状态 ==================================
    public void RestartGame()
    {
        // 1. 恢复游戏速度
        Time.timeScale = 1;
        NormalEnemyPool.Clear();
        FastEnemyPool.Clear();
        TankEnemyPool.Clear();
        // 2. 先销毁所有实体，回收对象到池
        // 修复：先复制实体列表的副本，避免遍历过程中修改原列表导致的枚举异常
        var tempEntities = new List<Entity>(_entities);
        foreach (Entity entity in tempEntities)
        {
            if (entity.HasComponent<ViewComponent>())
            {
                ViewComponent view = entity.GetComponent<ViewComponent>();
                if (view != null && view.GameObject != null)
                {
                    DestroyEntity(entity);
                }
            }
        }
        
        // 3. 清空所有对象池，销毁所有残留对象
        NormalBulletPool.Clear();
        SlowBulletPool.Clear();
        ChainLightningBulletPool.Clear();
        AOEBulletPool.Clear();
        EnemyPool.Clear();
        
        // 4. 清空状态，重新初始化
        _entities.Clear();
        _systems.Clear();
        Score = 0;
        InitGame();
        
        // 修复：重启时，完全恢复相机的初始状态，包括位置、Culling Mask、正交大小
        if (_cachedCamera != null)
        {
            _cachedCamera.transform.position = _cachedCameraPosition;
            _cachedCamera.cullingMask = _cachedCullingMask;
            _cachedOrthographicSize = _cachedCamera.orthographicSize;
        }
        
        // 5. 隐藏结束面板
        UIManager.Instance.GameOverPanel.SetActive(false);
    }
    
    // ================================== 核心修复：统一销毁方法，根据子弹类型回收到对应对象池 ==================================
    public void DestroyEntity(Entity entity)
    {
        if (entity.HasComponent<ViewComponent>())
        {
            ViewComponent view = entity.GetComponent<ViewComponent>();
            if (view != null && view.GameObject != null)
            {
                // 自动判断实体类型，回收到对应的对象池
                if (entity.HasComponent<BulletComponent>())
                {
                    // 修复：根据子弹类型，回收到对应的独立对象池
                    BulletComponent bulletComp = entity.GetComponent<BulletComponent>();
                    switch(bulletComp.Type)
                    {
                        case BulletType.Normal:
                            NormalBulletPool.Release(view.GameObject);
                            break;
                        case BulletType.Slow:
                            SlowBulletPool.Release(view.GameObject);
                            break;
                        case BulletType.ChainLightning:
                            ChainLightningBulletPool.Release(view.GameObject);
                            break;
                        case BulletType.AOE:
                            AOEBulletPool.Release(view.GameObject);
                            break;
                        default:
                            NormalBulletPool.Release(view.GameObject);
                            break;
                    }
                }
                else if (entity.HasComponent<EnemyComponent>())
                {
                    // 敌人：根据类型回收到对应的独立对象池
                    EnemyComponent enemyComp = entity.GetComponent<EnemyComponent>();
                    switch(enemyComp.Type)
                    {
                        case EnemyType.Normal:
                            NormalEnemyPool.Release(view.GameObject);
                            break;
                        case EnemyType.Fast:
                            FastEnemyPool.Release(view.GameObject);
                            break;
                        case EnemyType.Tank:
                            TankEnemyPool.Release(view.GameObject);
                            break;
                        default:
                            NormalEnemyPool.Release(view.GameObject);
                            break;
                    }
                }
                else
                {
                    // 其他对象（比如玩家、道具）：正常销毁
                    Object.Destroy(view.GameObject);
                }
            }
        }
        
        // 原有实体移除逻辑（保留）
        _entities.Remove(entity);
    }
}