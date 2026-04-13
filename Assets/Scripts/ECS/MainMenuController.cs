using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject MainPanel;           // 主界面
    public GameObject CharSelectPanel;     // 选人界面
    public GameObject TalentPanel;         // 天赋界面
    public GameObject HistoryPanel;        // 历史记录
    public GameObject SettingsPanel;       // 设置界面

    [Header("Transition Settings")]
    public string BattleSceneName = "BattleScene"; // 战斗场景名称

    private GameObject _currentPanel;

    void Start()
    {
        // ==========================================
        // 【核心修复：场景初始化安全锁】
        // ==========================================
        // 1. 强制恢复时间流动，防止按钮动画被冻结
        Time.timeScale = 1f;

        // 2. 强制解锁并显示鼠标指针
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 3. 强制关闭所有子级面板，防止透明背景拦截 UI 射线点击！
        if (CharSelectPanel != null) CharSelectPanel.SetActive(false);
        if (TalentPanel != null) TalentPanel.SetActive(false);
        if (HistoryPanel != null) HistoryPanel.SetActive(false);
        if (SettingsPanel != null) SettingsPanel.SetActive(false);

        // 初始化完毕后，干净地打开主界面
        ShowPanel(MainPanel);
        
        // 确保 GameDataManager 已启动
        if (GameDataManager.Instance == null)
        {
            Debug.LogWarning("未发现 GameDataManager，请确保主菜单场景中有该单例。");
        }
    }

    // ==========================================
    // 核心面板切换逻辑
    // ==========================================

    public void ShowPanel(GameObject targetPanel)
    {
        if (_currentPanel != null) _currentPanel.SetActive(false);
        
        targetPanel.SetActive(true);
        _currentPanel = targetPanel;
    }

    // 供主界面按钮调用
    public void OnStartButtonClick() => ShowPanel(CharSelectPanel);
    public void OnTalentButtonClick() => ShowPanel(TalentPanel);
    public void OnHistoryButtonClick() => ShowPanel(HistoryPanel);
    public void OnSettingsButtonClick() => ShowPanel(SettingsPanel);
    
    public void OnBackToMainClick() 
    {
        // 返回主菜单时同样强制清理一遍
        if (CharSelectPanel != null) CharSelectPanel.SetActive(false);
        if (TalentPanel != null) TalentPanel.SetActive(false);
        if (HistoryPanel != null) HistoryPanel.SetActive(false);
        if (SettingsPanel != null) SettingsPanel.SetActive(false);

        ShowPanel(MainPanel);
    }

    public void OnExitButtonClick()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // ==========================================
    // 角色选择与场景跳转
    // ==========================================

    public void SelectCharacter(int classIndex)
    {
        if (GameDataManager.Instance == null) return;

        // 映射枚举类型
        PlayerClass selected = (PlayerClass)classIndex;
        GameDataManager.Instance.SelectedCharacter = selected;

        Debug.Log($"已选择角色: {selected}，准备进入战场...");
        
        // 载入战斗场景前，再次确保时间正常
        Time.timeScale = 1f;
        SceneManager.LoadScene(BattleSceneName);
    }
}