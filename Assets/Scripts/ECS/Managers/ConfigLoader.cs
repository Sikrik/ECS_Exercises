using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class ConfigLoader
{
    public static GameConfig Load()
    {
        GameConfig config = new GameConfig();

        // 1. 加载全局基础设置 (game_config.csv)
        TextAsset baseCsv = Resources.Load<TextAsset>("game_config");
        if (baseCsv != null) ParseBaseSettings(config, baseCsv);

        // 2. 加载敌人特定配方 (Enemy_config.csv)
        TextAsset enemyCsv = Resources.Load<TextAsset>("Enemy_config");
        if (enemyCsv != null) ParseEnemyRecipes(config, enemyCsv);

        return config;
    }

    private static void ParseBaseSettings(GameConfig config, TextAsset csv)
    {
        string[] lines = csv.text.Split('\n');
        var fields = typeof(GameConfig).GetFields();
        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            if (cols.Length < 2) continue;
            string key = cols[0].Trim();
            foreach (var f in fields)
            {
                if (f.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    f.SetValue(config, Convert.ChangeType(cols[1].Trim(), f.FieldType, CultureInfo.InvariantCulture));
                    break;
                }
            }
        }
    }

    private static void ParseEnemyRecipes(GameConfig config, TextAsset csv)
    {
        string[] lines = csv.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 1; i < lines.Length; i++) 
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] cols = line.Split(',');
            // 变更：判定列数由 5 变为 6
            if (cols.Length < 6) continue;

            EnemyData data = new EnemyData {
                Id = cols[0].Trim(),
                Health = float.Parse(cols[1].Trim(), CultureInfo.InvariantCulture),
                Speed = float.Parse(cols[2].Trim(), CultureInfo.InvariantCulture),
                Damage = int.Parse(cols[3].Trim(), CultureInfo.InvariantCulture),
                // 新增：解析第 5 列硬直数据
                HitRecoveryDuration = float.Parse(cols[4].Trim(), CultureInfo.InvariantCulture),
                EnemyDeathScore = int .Parse(cols[5].Trim(), CultureInfo.InvariantCulture),
                // 变更：特性清单移至第 6 列 (Index 5)
                Traits = string.IsNullOrWhiteSpace(cols[5]) ? 
                    new string[0] : 
                    cols[5].Trim().Split('|')
            };
            config.EnemyRecipes[data.Id] = data;
        }
    }
}