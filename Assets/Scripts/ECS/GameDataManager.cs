// 路径: Assets/Scripts/ECS/GameDataManager.cs
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class MatchRecord
{
    public string CharacterUsed;
    public int FinalScore;       
    public int WaveReached;      
    public bool IsVictory;       
    public string Date;          
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
    public List<TalentRecord> SavedTalents = new List<TalentRecord>();
    public List<MatchRecord> History = new List<MatchRecord>();
}

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    public GameSaveData SaveData { get; private set; }
    public Dictionary<string, int> TalentDict { get; private set; } = new Dictionary<string, int>();
    public PlayerClass SelectedCharacter = PlayerClass.Standard;

    // 👇 【核心修改1】：把配置表存放到全局管理器中
    public GameConfig Config { get; private set; }

    private string _saveFilePath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 👇 【核心修改2】：游戏一启动就加载配置
            Config = ConfigLoader.Load(); 

            _saveFilePath = Path.Combine(Application.persistentDataPath, "GameSave.json");
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

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
            SaveData = new GameSaveData();
        }

        TalentDict.Clear();
        foreach (var t in SaveData.SavedTalents)
        {
            TalentDict[t.TalentId] = t.Level;
        }
    }

    public void SaveGame()
    {
        SaveData.SavedTalents.Clear();
        foreach (var kvp in TalentDict)
        {
            SaveData.SavedTalents.Add(new TalentRecord { TalentId = kvp.Key, Level = kvp.Value });
        }

        string json = JsonUtility.ToJson(SaveData, true);
        File.WriteAllText(_saveFilePath, json);
        Debug.Log($"游戏已保存至: {_saveFilePath}");
    }

    public int GetTalentLevel(string talentId)
    {
        return TalentDict.TryGetValue(talentId, out int level) ? level : 0;
    }

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
        if (SaveData.History.Count > 50)
        {
            SaveData.History.RemoveAt(SaveData.History.Count - 1);
        }
        SaveGame();
    }
}