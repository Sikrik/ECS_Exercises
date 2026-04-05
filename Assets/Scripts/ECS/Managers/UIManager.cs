// UIManager.cs 完整代码
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    [Header("UI组件引用")]
    public Slider HealthSlider;       // 玩家血量条
    public TextMeshProUGUI ScoreText;  // 实时得分显示
    public GameObject GameOverPanel;  // 游戏结束面板
    public TextMeshProUGUI FinalScoreText;       // 最终得分显示
    void Awake()
    {
        // 单例模式，确保全局唯一
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        // 初始隐藏游戏结束面板
        GameOverPanel.SetActive(false);
    }
    void Update()
    {
        // 每帧更新UI（血量、得分）
        UpdateHealthUI();
        UpdateScoreUI();
    }
    /// <summary>
    /// 更新玩家血量UI
    /// </summary>
    void UpdateHealthUI()
    {
        // 避免空引用
        if (ECSManager.Instance?.PlayerEntity == null) return;
        if (!ECSManager.Instance.PlayerEntity.HasComponent<HealthComponent>()) return;
        
        var health = ECSManager.Instance.PlayerEntity.GetComponent<HealthComponent>();
        // 计算血量百分比，赋值给滑块
        HealthSlider.value = health.CurrentHealth / health.MaxHealth;
    }
    /// <summary>
    /// 更新得分UI
    /// </summary>
    void UpdateScoreUI()
    {
        // 实时显示当前得分
        ScoreText.text = $"得分: {ECSManager.Instance.Score}";
    }
    /// <summary>
    /// 玩家死亡时，显示游戏结束面板
    /// </summary>
    public void ShowGameOver()
    {
        GameOverPanel.SetActive(true);
        // 显示最终得分
        FinalScoreText.text = $"最终得分: {ECSManager.Instance.Score}";
    }
    
    public void OnRestartButtonClick()
    {
        // 1. 隐藏掉 Game Over 面板
        if (GameOverPanel != null) GameOverPanel.SetActive(false);

        // 2. 调用 ECSManager 的重启逻辑
        ECSManager.Instance.RestartGame();
    }
}