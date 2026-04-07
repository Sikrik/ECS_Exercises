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

    // 血量的脏标记缓存（血量目前依然使用每帧轮询更新）
    private float _lastHealthRatio = -1f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        if (GameOverPanel != null) GameOverPanel.SetActive(false);
    }

    // ==========================================
    // 事件监听注册与注销 (核心解耦部分)
    // ==========================================
    void OnEnable()
    {
        EventManager.AddListener<ScoreChangedEvent>(OnScoreChanged);
        EventManager.AddListener<GameOverEvent>(OnGameOver);
    }

    void OnDisable()
    {
        EventManager.RemoveListener<ScoreChangedEvent>(OnScoreChanged);
        EventManager.RemoveListener<GameOverEvent>(OnGameOver);
    }

    void Update()
    {
        // 现在的 Update 极其干净，只剩下血量的脏标记轮询
        UpdateHealthUI();
    }

    /// <summary>
    /// 更新玩家血量UI (脏标记模式)
    /// </summary>
    void UpdateHealthUI()
    {
        if (ECSManager.Instance?.PlayerEntity == null || !ECSManager.Instance.PlayerEntity.IsAlive) return;
        if (!ECSManager.Instance.PlayerEntity.HasComponent<HealthComponent>()) return;
        
        var health = ECSManager.Instance.PlayerEntity.GetComponent<HealthComponent>();
        float ratio = health.MaxHealth > 0 ? health.CurrentHealth / health.MaxHealth : 0;
        
        if (HealthSlider != null && Mathf.Abs(_lastHealthRatio - ratio) > 0.001f)
        {
            _lastHealthRatio = ratio;
            HealthSlider.value = ratio;
        }
    }

    // ==========================================
    // 事件响应回调
    // ==========================================
    
    /// <summary>
    /// 响应得分变化事件
    /// </summary>
    private void OnScoreChanged(ScoreChangedEvent evt)
    {
        if (ScoreText != null)
        {
            ScoreText.text = $"得分: {evt.NewScore}";
        }
    }

    /// <summary>
    /// 响应游戏结束事件
    /// </summary>
    private void OnGameOver(GameOverEvent evt)
    {
        if (GameOverPanel != null) GameOverPanel.SetActive(true);
        if (FinalScoreText != null)
        {
            FinalScoreText.text = $"最终得分: {ECSManager.Instance.Score}";
        }
    }
    
    // ==========================================
    // UI 按钮交互
    // ==========================================
    public void OnRestartButtonClick()
    {
        if (GameOverPanel != null) GameOverPanel.SetActive(false);
        ECSManager.Instance.RestartGame();
    }
}