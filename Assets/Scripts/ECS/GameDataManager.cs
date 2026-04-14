// 路径: Assets/Scripts/ECS/GameDataManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

// ==========================================
// 数据模型定义（建议后期单独抽离到 GameSaveModels.cs）
// ==========================================
[Serializable]
public class MatchRecord { public string CharacterUsed; public int FinalScore; public int WaveReached; public bool IsVictory; public string Date; }
[Serializable]
public class TalentRecord { public string TalentId; public int Level; }
[Serializable]
public class GameSaveData { public int TotalGold; public List<TalentRecord> SavedTalents = new List<TalentRecord>(); public List<MatchRecord> History = new List<MatchRecord>(); }

// ==========================================
// 纯净的局外数据管理器
// ==========================================
public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    public GameSaveData SaveData { get; private set; }
    public Dictionary<string, int> TalentDict { get; private set; } = new Dictionary<string, int>();
    public PlayerClass SelectedCharacter = PlayerClass.Standard;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeData()
    {
        // 向 SaveManager 请求数据
        SaveData = SaveManager.Load();

        TalentDict.Clear();
        foreach (var t in SaveData.SavedTalents)
        {
            TalentDict[t.TalentId] = t.Level;
        }
    }

    public void SaveGame()
    {
        // 整理当前内存数据
        SaveData.SavedTalents.Clear();
        foreach (var kvp in TalentDict)
        {
            SaveData.SavedTalents.Add(new TalentRecord { TalentId = kvp.Key, Level = kvp.Value });
        }
        // 委托 SaveManager 写入磁盘
        SaveManager.Save(SaveData);
    }

    // --- 以下为纯粹的业务逻辑接口 ---

    public int GetTalentLevel(string talentId) => TalentDict.TryGetValue(talentId, out int level) ? level : 0;

    public bool TryUpgradeTalent(string talentId, int cost)
    {
        if (SaveData.TotalGold >= cost)
        {
            SaveData.TotalGold -= cost;
            if (!TalentDict.ContainsKey(talentId)) TalentDict[talentId] = 0;
            TalentDict[talentId]++;
            
            SaveGame(); 
            return true;
        }
        return false;
    }

    public void AddGold(int amount)
    {
        SaveData.TotalGold += amount;
        SaveGame();
    }

    public void AddMatchRecord(MatchRecord record)
    {
        SaveData.History.Insert(0, record); 
        if (SaveData.History.Count > 50) SaveData.History.RemoveAt(SaveData.History.Count - 1);
        SaveGame();
    }
}