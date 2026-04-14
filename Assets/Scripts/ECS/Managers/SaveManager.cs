// 路径: Assets/Scripts/ECS/Core/Managers/SaveManager.cs
using System;
using System.IO;
using UnityEngine;

public static class SaveManager
{
    private static readonly string SaveFilePath = Path.Combine(Application.persistentDataPath, "GameSave.json");

    public static GameSaveData Load()
    {
        if (File.Exists(SaveFilePath))
        {
            try
            {
                string json = File.ReadAllText(SaveFilePath);
                var data = JsonUtility.FromJson<GameSaveData>(json);
                return data ?? new GameSaveData();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] 存档读取失败，已重置存档: {e.Message}");
            }
        }
        return new GameSaveData(); // 若无文件则返回全新存档
    }

    public static void Save(GameSaveData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SaveFilePath, json);
            Debug.Log($"[SaveManager] 游戏已成功保存至: {SaveFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] 存档写入失败: {e.Message}");
        }
    }
}