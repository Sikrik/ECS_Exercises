// EnemySpawnSystem.cs 修复版本，适配协同文档的新代码
using System.Collections.Generic;
using UnityEngine;
public class EnemySpawnSystem : SystemBase
{
    // 生成计时器
    private float _spawnTimer;
    // 敌人预制体引用（保留兼容，实际已改用对象池）
    private GameObject _enemyPrefab;
    
    // 构造函数
    public EnemySpawnSystem(List<Entity> entities, GameObject enemyPrefab) : base(entities)
    {
        _enemyPrefab = enemyPrefab;
        _spawnTimer = 0;
    }
    public override void Update(float deltaTime)
    {
        // 游戏暂停时不生成敌人
        if (Time.timeScale <= 0) return;
        
        _spawnTimer += deltaTime;
        GameConfig config = ECSManager.Instance.Config;
        
        // 动态调整生成间隔：得分越高，生成越快，使用可配置的难度参数
        float spawnInterval = Mathf.Max(
            config.MinSpawnInterval, 
            config.InitialSpawnInterval - (ECSManager.Instance.Score * config.SpawnIntervalDecrease)
        );
        // 达到生成间隔，生成敌人
        if (_spawnTimer >= spawnInterval)
        {
            SpawnEnemyAtCameraOuterCircle();
            _spawnTimer = 0;
        }
    }
    /// <summary>
    /// 在相机视野外的随机位置生成敌人（核心生成逻辑）
    /// </summary>
    void SpawnEnemyAtCameraOuterCircle()
    {
        GameConfig config = ECSManager.Instance.Config;
        if (config == null) return;
        // 1. 获取相机边界，计算生成范围（在相机视野外一圈）
        Camera mainCam = Camera.main;
        if (mainCam == null) 
        {
            // 兼容非MainCamera标签的相机
            mainCam = Object.FindObjectOfType<Camera>();
            if (mainCam == null) return;
        }
        // 计算相机正交视野的边界
        float camHeight = mainCam.orthographicSize;
        float camWidth = camHeight * mainCam.aspect;
        // 生成半径：相机视野外扩展一段距离，避免直接出现在视野里
        float spawnRadius = Mathf.Max(camWidth, camHeight) + 2f;
        // 2. 随机生成角度和位置（在相机外的圆形区域）
        float randomAngle = Random.Range(0, Mathf.PI * 2);
        float spawnX = Mathf.Cos(randomAngle) * spawnRadius;
        float spawnY = Mathf.Sin(randomAngle) * spawnRadius;
        // 3. 根据得分动态选择敌人类型（难度递进）
        EnemyType enemyType;
        float random = Random.value;
        
        // 得分越高，强敌人概率越高
        if (ECSManager.Instance.Score > 100 && random < 0.2f)
        {
            // 得分>100：20%概率生成坦克敌人（血厚、移速慢）
            enemyType = EnemyType.Tank;
        }
        else if (ECSManager.Instance.Score > 50 && random < 0.3f)
        {
            // 得分>50：30%概率生成快速敌人（血少、移速快）
            enemyType = EnemyType.Fast;
        }
        else
        {
            // 默认生成普通敌人
            enemyType = EnemyType.Normal;
        }
        // 4. 根据敌人类型获取对应对象池的预制体，并设置属性
        GameObject enemyGo = null;
        float enemyHealth = config.EnemyMaxHealth;
        float enemySpeed = config.EnemyMoveSpeed;
        float enemyCollisionRadius = config.EnemyCollisionRadius;
        switch (enemyType)
        {
            case EnemyType.Fast:
                // 快速敌人：从快速敌人对象池获取
                enemyGo = ECSManager.Instance.FastEnemyPool.Get();
                enemyHealth = config.FastEnemyMaxHealth;
                enemySpeed = config.FastEnemySpeed;
                enemyCollisionRadius = config.FastEnemyCollisionRadius;
                break;
            
            case EnemyType.Tank:
                // 坦克敌人：从坦克敌人对象池获取
                enemyGo = ECSManager.Instance.TankEnemyPool.Get();
                enemyHealth = config.TankEnemyMaxHealth;
                enemySpeed = config.TankEnemySpeed;
                enemyCollisionRadius = config.TankEnemyCollisionRadius;
                break;
            
            case EnemyType.Normal:
            default:
                // 普通敌人：从普通敌人对象池获取
                enemyGo = ECSManager.Instance.NormalEnemyPool.Get();
                break;
        }
        // 防御：对象池获取失败时，用默认预制体兜底
        if (enemyGo == null)
        {
            enemyGo = Object.Instantiate(_enemyPrefab);
            Debug.LogWarning($"敌人对象池获取失败，使用默认预制体生成{enemyType}敌人");
        }
        
        // 自动同步碰撞半径与Sprite视觉大小，解决视觉与碰撞体不匹配问题
        if (enemyGo.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
        {
            // 获取Sprite的世界空间大小，计算碰撞半径（取宽高的最小值的一半，保证圆形碰撞贴合Sprite）
            float spriteWidth = spriteRenderer.bounds.size.x;
            float spriteHeight = spriteRenderer.bounds.size.y;
            // 自动覆盖配置的半径，保证碰撞体与视觉大小完全匹配
            enemyCollisionRadius = Mathf.Min(spriteWidth, spriteHeight) * 0.5f;
        }
        
        // 5. 创建敌人实体并添加组件
        Entity enemy = ECSManager.Instance.CreateEntity();
        // 位置组件（生成在随机位置）
        enemy.AddComponent(new PositionComponent(spawnX, spawnY, 0));
        // 速度组件（初始为0，由AI系统控制移动）
        enemy.AddComponent(new VelocityComponent(0, 0, 0));
        // 敌人核心组件（包含类型、伤害、冷却）
        enemy.AddComponent(new EnemyComponent()
        {
            Damage = config.EnemyDamage,       // 所有敌人伤害暂时统一，可后续拆分
            AttackCooldown = config.EnemyAttackCooldown,
            CurrentCooldown = 0,
            Type = enemyType                   // 标记敌人类型
        });
        // 血量组件（不同敌人血量不同）
        enemy.AddComponent(new HealthComponent(enemyHealth));
        // 碰撞组件（不同敌人碰撞半径不同）
        enemy.AddComponent(new CollisionComponent(enemyCollisionRadius));
        // 6. 设置敌人GameObject的位置，并绑定到视图组件
        enemyGo.transform.position = new Vector3(spawnX, spawnY, 0);
        enemyGo.transform.rotation = Quaternion.identity;
        enemyGo.SetActive(true); // 确保对象池取出的对象是激活状态
        enemy.AddComponent(new ViewComponent(enemyGo));
    }
}