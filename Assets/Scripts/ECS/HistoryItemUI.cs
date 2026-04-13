using UnityEngine;
using TMPro; // 如果你使用的是原生 Text，请换成 UnityEngine.UI

public class HistoryItemUI : MonoBehaviour
{
    public TextMeshProUGUI DateText;
    public TextMeshProUGUI CharacterText;
    public TextMeshProUGUI WaveText;
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI ResultText;

    // 接收从管理器传来的数据并刷新 UI
    public void Setup(MatchRecord record)
    {
        DateText.text = record.Date;
        CharacterText.text = $"使用角色: {record.CharacterUsed}";
        WaveText.text = $"存活波次: {record.WaveReached}";
        ScoreText.text = $"得分: {record.FinalScore}";
        
        if (record.IsVictory)
        {
            ResultText.text = "通关";
            ResultText.color = Color.green;
        }
        else
        {
            ResultText.text = "战败";
            ResultText.color = Color.red;
        }
    }
}