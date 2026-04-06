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
        // 每帧同步血量和得分
        UpdateHealthUI();
        UpdateScoreUI();
    }

    /// <summary>
    /// 更新玩家血量UI
    /// </summary>
    void UpdateHealthUI()
    {
        // 安全检查：确保玩家实体存在且拥有血量组件
        if (ECSManager.Instance?.PlayerEntity == null) return;
        if (!ECSManager.Instance.PlayerEntity.HasComponent<HealthComponent>()) return;
        
        var health = ECSManager.Instance.PlayerEntity.GetComponent<HealthComponent>();
        
        // 计算血量百分比 (0 到 1)，赋值给滑块
        if (HealthSlider != null)
        {
            // 防止除零错误，确保 MaxHealth 正常
            float ratio = health.MaxHealth > 0 ? health.CurrentHealth / health.MaxHealth : 0;
            HealthSlider.value = ratio;
        }
    }

    /// <summary>
    /// 更新得分UI
    /// </summary>
    void UpdateScoreUI()
    {
        if (ScoreText != null)
            ScoreText.text = $"得分: {ECSManager.Instance.Score}";
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