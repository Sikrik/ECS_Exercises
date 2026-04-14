using UnityEngine;

public class HistoryUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject HistoryItemPrefab; // 单条记录的预制体
    public Transform ContentParent;      // ScrollView 的 Content 节点

    // 每次面板被激活显示时，自动执行刷新
    void OnEnable()
    {
        RefreshHistoryList();
    }

    public void RefreshHistoryList()
    {
        // 防御性检查
        if (GameDataManager.Instance == null || HistoryItemPrefab == null || ContentParent == null) return;

        // 1. 清空旧的列表内容 (防止每次打开都重复叠加)
        foreach (Transform child in ContentParent)
        {
            Destroy(child.gameObject);
        }

        // 2. 读取存档中的历史记录
        var historyList = GameDataManager.Instance.SaveData.History;

        if (historyList.Count == 0)
        {
            Debug.Log("暂无历史记录");
            return;
        }

        // 3. 遍历数据，动态生成 UI 预制体
        foreach (var record in historyList)
        {
            GameObject go = Instantiate(HistoryItemPrefab, ContentParent);
            HistoryItemUI itemUI = go.GetComponent<HistoryItemUI>();
            
            if (itemUI != null)
            {
                itemUI.Setup(record);
            }
        }
    }
}