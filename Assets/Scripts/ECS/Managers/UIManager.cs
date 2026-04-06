using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    [Header("UI组件引用")]
    public Slider HealthSlider;       // 玩家血量条 (Slider)
    public TextMeshProUGUI ScoreText;  // 实时得分显示 (TMP)
    public GameObject GameOverPanel;  // 游戏结束面板 (Panel)
    public TextMeshProUGUI FinalScoreText; // 最终得分显示 (TMP)

    // 👇 新增缓存变量：用于脏标记检测，避免每帧重复拼接字符串或刷新组件
    private int _lastScore = -1;
    private float _lastHealthRatio = -1f;

    void Awake()
    {
        // 单例模式，确保全局唯一
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        // 初始隐藏游戏结束面板
        if (GameOverPanel != null) GameOverPanel.SetActive(false);
    }

    void Update()
    {
        // 每帧检查血量和得分是否需要同步
        UpdateHealthUI();
        UpdateScoreUI();
    }

    /// <summary>
    /// 更新玩家血量UI
    /// </summary>
    void UpdateHealthUI()
    {
        // 安全检查：确保玩家实体存在且拥有血量组件
        if (ECSManager.Instance?.PlayerEntity == null || !ECSManager.Instance.PlayerEntity.IsAlive) return;
        if (!ECSManager.Instance.PlayerEntity.HasComponent<HealthComponent>()) return;
        
        var health = ECSManager.Instance.PlayerEntity.GetComponent<HealthComponent>();
        
        // 计算血量百分比 (0 到 1)
        float ratio = health.MaxHealth > 0 ? health.CurrentHealth / health.MaxHealth : 0;
        
        // 👇 脏标记检测：如果比例有变化才更新滑块（加入 0.001 容差防止浮点数精度误差频繁触发）
        if (HealthSlider != null && Mathf.Abs(_lastHealthRatio - ratio) > 0.001f)
        {
            _lastHealthRatio = ratio;
            HealthSlider.value = ratio;
        }
    }

    /// <summary>
    /// 更新得分UI
    /// </summary>
    void UpdateScoreUI()
    {
        // 👇 脏标记检测：只有当分数真的发生变化时，才执行耗费性能的 $"" 字符串拼接
        if (ScoreText != null && _lastScore != ECSManager.Instance.Score)
        {
            _lastScore = ECSManager.Instance.Score;
            ScoreText.text = $"得分: {_lastScore}";
        }
    }

    /// <summary>
    /// 玩家死亡时调用，显示结算面板
    /// </summary>
    public void ShowGameOver()
    {
        if (GameOverPanel != null) GameOverPanel.SetActive(true);
        if (FinalScoreText != null)
            FinalScoreText.text = $"最终得分: {ECSManager.Instance.Score}";
    }
    
    public void OnRestartButtonClick()
    {
        // 重置游戏状态并调用 ECSManager 的重启逻辑
        if (GameOverPanel != null) GameOverPanel.SetActive(false);
        ECSManager.Instance.RestartGame();
    }
}