// 路径: Assets/Scripts/ECS/BattleManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 战斗业务管理器：专职管理单局游戏的配置、状态、得分和流程流转
/// </summary>
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Header("战斗配置")]
    public PlayerClass SelectedCharacter = PlayerClass.Standard;
    public GameConfig Config;
    public GameObject PlayerPrefab;

    [Header("实时状态")]
    public int Score = 0; 
    public int CurrentWave = 1;
    public int MaxWave = 1;

    void Awake()
    {
        Instance = this;
        
        // 1. 获取选人信息（独立判断 GameDataManager，解耦场景依赖）
        if (GameDataManager.Instance != null)
        {
            SelectedCharacter = GameDataManager.Instance.SelectedCharacter;
        }
        else
        {
            Debug.LogWarning("未检测到 GameDataManager，将使用默认角色。");
        }

        // 2. 获取配置信息（独立判断 ConfigManager，支持战斗场景直接运行测试）
        if (ConfigManager.Instance != null)
        {
            Config = ConfigManager.Instance.Config;
        }
        else
        {
            Debug.LogWarning("未检测到 ConfigManager，使用 ConfigLoader 临时加载配置。");
            Config = ConfigLoader.Load(); 
        }
    }

    void Start()
    {
        // 游戏开始时，通知工厂生成玩家实体，并把引用存放到纯净的 ECSManager 中
        if (PlayerPrefab == null) Debug.LogError("PlayerPrefab 丢失！");
        
        // 这一步把玩家实体交还给 ECSManager 保管
        ECSManager.Instance.SetPlayerEntity(PlayerFactory.Create(SelectedCharacter, PlayerPrefab, Config));
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu"); 
    }
}