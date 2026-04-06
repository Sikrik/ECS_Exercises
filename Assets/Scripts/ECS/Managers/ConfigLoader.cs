using System;
using System.Collections.Generic;
using UnityEngine;

public static class ConfigLoader
{
    public static GameConfig Load(string path)
    {
        TextAsset csvText = Resources.Load<TextAsset>(path);
        if (csvText == null) return null;

        GameConfig config = new GameConfig();
        string[] lines = csvText.text.Split('\n');

        // 这里仅展示敌人配方解析的核心逻辑 (假设从特定行开始或使用新的 CSV 结构)
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] cols = line.Split(',');
            // 假设 CSV 格式: ID, Health, Speed, Damage, Traits
            if (cols.Length < 5) continue;

            EnemyData data = new EnemyData {
                Id = cols[0],
                Health = float.Parse(cols[1]),
                Speed = float.Parse(cols[2]),
                Damage = int.Parse(cols[3]),
                Traits = cols[4].Split('|') // 使用 | 分隔多个特性
            };
            config.EnemyRecipes[data.Id] = data;
        }
        return config;
    }
}