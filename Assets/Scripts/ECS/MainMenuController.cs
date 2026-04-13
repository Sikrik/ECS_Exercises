using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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
        // 初始状态：只显示主界面
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
        // 确保所有可能打开的子页面强制设为非活跃
        if (CharSelectPanel != null) CharSelectPanel.SetActive(false);
        if (TalentPanel != null) TalentPanel.SetActive(false);
        if (HistoryPanel != null) HistoryPanel.SetActive(false);
        if (SettingsPanel != null) SettingsPanel.SetActive(false);

        // 强制打开主界面，并更新状态
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

    /// <summary>
    /// 选人界面按钮点击事件
    /// </summary>
    /// <param name="classIndex">0: Standard, 1: Heavy, 2: Agile</param>
    public void SelectCharacter(int classIndex)
    {
        if (GameDataManager.Instance == null) return;

        // 映射枚举类型
        PlayerClass selected = (PlayerClass)classIndex;
        GameDataManager.Instance.SelectedCharacter = selected;

        Debug.Log($"已选择角色: {selected}，准备进入战场...");
        
        // 载入战斗场景
        SceneManager.LoadScene(BattleSceneName);
    }
}