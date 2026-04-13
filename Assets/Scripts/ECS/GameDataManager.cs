using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// ==========================================
// 1. 存档数据模型
// ==========================================

[Serializable]
public class MatchRecord
{
    public string CharacterUsed; // 使用的角色 (例如 "Standard")
    public int FinalScore;       // 最终得分
    public int WaveReached;      // 存活波次
    public bool IsVictory;       // 是否通关
    public string Date;          // 游玩日期
}

[Serializable]
public class TalentRecord
{
    public string TalentId;
    public int Level;
}

[Serializable]
public class GameSaveData
{
    public int TotalGold;
    
    // Unity 的 JsonUtility 无法直接序列化字典，所以存档时使用 List
    public List<TalentRecord> SavedTalents = new List<TalentRecord>();
    public List<MatchRecord> History = new List<MatchRecord>();
}

// ==========================================
// 2. 全局数据管理器 (单例 + 跨场景)
// ==========================================

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    // 内存中的存档数据
    public GameSaveData SaveData { get; private set; }
    
    // 内存中使用的字典，方便 O(1) 查找天赋等级
    public Dictionary<string, int> TalentDict { get; private set; } = new Dictionary<string, int>();

    // 暂存：主菜单选中的角色，带入战斗场景
    public PlayerClass SelectedCharacter = PlayerClass.Standard;

    private string _saveFilePath;

    void Awake()
    {
        // 标准单例与跨场景保留
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 存档路径：无论在 Editor 还是打包后，这里都是安全的持久化读写目录
            _saveFilePath = Path.Combine(Application.persistentDataPath, "GameSave.json");
            
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ==========================================
    // 核心功能：读取与保存
    // ==========================================

    public void LoadData()
    {
        if (File.Exists(_saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(_saveFilePath);
                SaveData = JsonUtility.FromJson<GameSaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"存档读取失败，已重置存档: {e.Message}");
                SaveData = new GameSaveData();
            }
        }
        else
        {
            // 首次游戏，创建新存档
            SaveData = new GameSaveData();
        }

        // 将 List 转换为 Dictionary 供内存快速使用
        TalentDict.Clear();
        foreach (var t in SaveData.SavedTalents)
        {
            TalentDict[t.TalentId] = t.Level;
        }
    }

    public void SaveGame()
    {
        // 保存前，将内存中的 Dictionary 同步回 List
        SaveData.SavedTalents.Clear();
        foreach (var kvp in TalentDict)
        {
            SaveData.SavedTalents.Add(new TalentRecord { TalentId = kvp.Key, Level = kvp.Value });
        }

        string json = JsonUtility.ToJson(SaveData, true); // true 表示格式化输出方便调试查看
        File.WriteAllText(_saveFilePath, json);
        Debug.Log($"游戏已保存至: {_saveFilePath}");
    }

    // ==========================================
    // 提供给外部调用的 API
    // ==========================================

    // 获取某个局外天赋的等级
    public int GetTalentLevel(string talentId)
    {
        return TalentDict.TryGetValue(talentId, out int level) ? level : 0;
    }

    // 升级某个天赋并扣除金币
    public bool TryUpgradeTalent(string talentId, int cost)
    {
        if (SaveData.TotalGold >= cost)
        {
            SaveData.TotalGold -= cost;
            if (!TalentDict.ContainsKey(talentId)) TalentDict[talentId] = 0;
            TalentDict[talentId]++;
            
            SaveGame(); // 升级后立即保存
            return true;
        }
        return false;
    }

    // 增加金币 (战斗结束结算时调用)
    public void AddGold(int amount)
    {
        SaveData.TotalGold += amount;
        SaveGame();
    }

    // 添加历史记录 (战斗结束结算时调用)
    public void AddMatchRecord(MatchRecord record)
    {
        SaveData.History.Insert(0, record); // 最新记录插在最前面
        
        // 限制历史记录最多保存 50 条，防止存档过大
        if (SaveData.History.Count > 50)
        {
            SaveData.History.RemoveAt(SaveData.History.Count - 1);
        }
        
        SaveGame();
    }
}