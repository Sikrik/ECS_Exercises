using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 表现层视图 (View)
/// 职责：只提供修改 UI 元素的公共方法，不包含任何业务逻辑、不监听任何事件、没有 Update()。
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    [Header("UI组件引用")]
    public Slider HealthSlider;       
    public TextMeshProUGUI ScoreText;  
    public GameObject GameOverPanel;  
    public TextMeshProUGUI FinalScoreText; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        if (GameOverPanel != null) GameOverPanel.SetActive(false);
    }

    // ==========================================
    // 纯渲染接口，供 ECS 的 UISyncSystem 调用
    // ==========================================
    
    public void UpdateHealth(float current, float max)
    {
        if (HealthSlider != null)
        {
            HealthSlider.value = max > 0 ? current / max : 0;
        }
    }

    public void UpdateScore(int score)
    {
        if (ScoreText != null) ScoreText.text = $"得分: {score}";
    }

    public void ShowGameOver(int finalScore)
    {
        if (GameOverPanel != null) GameOverPanel.SetActive(true);
        if (FinalScoreText != null) FinalScoreText.text = $"最终得分: {finalScore}";
    }
    
    public void OnRestartButtonClick()
    {
        if (GameOverPanel != null) GameOverPanel.SetActive(false);
        ECSManager.Instance.RestartGame();
    }
    
    // 1. 【新增】敌人数量的 UI 组件引用
    public TextMeshProUGUI EnemyCountText;
    public void UpdateEnemyCount(int count)
        {
            if (EnemyCountText != null) 
            {
                EnemyCountText.text = $"在场敌人: {count}";
            }
        }
}