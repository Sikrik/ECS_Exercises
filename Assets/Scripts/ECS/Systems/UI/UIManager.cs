using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 表现层视图 (View)
/// 负责管理游戏中的所有 UI 元素显示和更新
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("战斗 UI")]
    [Tooltip("玩家血量条")]
    public Slider HealthSlider;
    
    [Tooltip("实时得分文本")]
    public TextMeshProUGUI ScoreText;
    
    [Tooltip("在场敌人数量文本")]
    public TextMeshProUGUI EnemyCountText;

    [Tooltip("准星 UI")]
    public RectTransform CrosshairUI; // 新增：准星UI引用

    [Header("失败界面")]
    [Tooltip("失败面板")]
    public GameObject GameOverPanel;
    
    [Tooltip("失败时显示的最终得分")]
    public TextMeshProUGUI FinalScoreText;

    [Header("波次与胜利 UI")]
    [Tooltip("当前波次文本")]
    public TextMeshProUGUI WaveText;
    
    [Tooltip("胜利面板")]
    public GameObject VictoryPanel;
    
    [Tooltip("胜利时显示的最终得分")]
    public TextMeshProUGUI VictoryScoreText;

    void Awake()
    {
        InitializeSingleton();
        HideAllPanels();
    }

    /// <summary>
    /// 初始化单例模式
    /// </summary>
    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 隐藏所有面板
    /// </summary>
    private void HideAllPanels()
    {
        if (GameOverPanel != null)
        {
            GameOverPanel.SetActive(false);
        }
        
        if (VictoryPanel != null)
        {
            VictoryPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 控制准星的显示与隐藏
    /// </summary>
    public void SetCrosshairActive(bool isActive)
    {
        if (CrosshairUI != null && CrosshairUI.gameObject.activeSelf != isActive)
        {
            CrosshairUI.gameObject.SetActive(isActive);
        }
    }

    /// <summary>
    /// 更新准星位置（屏幕坐标）
    /// </summary>
    public void UpdateCrosshairPosition(Vector2 screenPos)
    {
        if (CrosshairUI != null)
        {
            CrosshairUI.position = screenPos; 
        }
    }

    /// <summary>
    /// 更新玩家血量显示
    /// </summary>
    /// <param name="current">当前血量</param>
    /// <param name="max">最大血量</param>
    public void UpdateHealth(float current, float max)
    {
        if (HealthSlider == null) return;
        
        HealthSlider.value = max > 0 ? current / max : 0;
    }

    /// <summary>
    /// 更新得分显示
    /// </summary>
    /// <param name="score">当前得分</param>
    public void UpdateScore(int score)
    {
        if (ScoreText == null) return;
        
        ScoreText.text = $"得分: {score}";
    }

    /// <summary>
    /// 更新在场敌人数量显示
    /// </summary>
    /// <param name="count">敌人数量</param>
    public void UpdateEnemyCount(int count)
    {
        if (EnemyCountText == null) return;
        
        EnemyCountText.text = $"在场敌人: {count}";
    }

    /// <summary>
    /// 更新波次显示
    /// </summary>
    /// <param name="current">当前波次</param>
    /// <param name="max">总波次</param>
    public void UpdateWave(int current, int max)
    {
        if (WaveText == null) return;
        
        WaveText.text = $"波次: {current} / {max}";
    }

    /// <summary>
    /// 显示游戏失败界面
    /// </summary>
    /// <param name="finalScore">最终得分</param>
    public void ShowGameOver(int finalScore)
    {
        Cursor.visible = true; // 确保鼠标显示
        if (GameOverPanel != null)
        {
            GameOverPanel.SetActive(true);
        }
        
        if (FinalScoreText != null)
        {
            FinalScoreText.text = $"最终得分: {finalScore}";
        }
    }

    /// <summary>
    /// 显示游戏胜利界面
    /// </summary>
    /// <param name="finalScore">最终得分</param>
    public void ShowVictory(int finalScore)
    {
        Cursor.visible = true; // 确保鼠标显示
        if (VictoryPanel != null)
        {
            VictoryPanel.SetActive(true);
        }
        
        if (VictoryScoreText != null)
        {
            VictoryScoreText.text = $"游戏通关！\n最终得分: {finalScore}";
        }
    }

    /// <summary>
    /// 重新开始游戏（按钮事件）
    /// </summary>
    public void OnRestartButtonClick()
    {
        HideAllPanels();
        ECSManager.Instance.RestartGame();
    }
}