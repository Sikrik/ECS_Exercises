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
        
        // 👇 新增子弹配表读取
        TextAsset bulletCsv = Resources.Load<TextAsset>("Bullet_config");
        if (bulletCsv != null) ParseBulletRecipes(config, bulletCsv);

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
            // 把这里的判断改成 7 列
            if (cols.Length < 7) continue; 

            EnemyData data = new EnemyData {
                Id = cols[0].Trim(),
                Health = float.Parse(cols[1].Trim(), CultureInfo.InvariantCulture),
                Speed = float.Parse(cols[2].Trim(), CultureInfo.InvariantCulture),
                Damage = int.Parse(cols[3].Trim(), CultureInfo.InvariantCulture),
                HitRecoveryDuration = float.Parse(cols[4].Trim(), CultureInfo.InvariantCulture),
                // 分数在第 6 列，索引是 5
                EnemyDeathScore = int.Parse(cols[5].Trim(), CultureInfo.InvariantCulture),
                // 特性现在被挤到了第 7 列，索引应该是 6！
                Traits = string.IsNullOrWhiteSpace(cols[6]) ? 
                    new string[0] : 
                    cols[6].Trim().Split('|')
            };
            config.EnemyRecipes[data.Id] = data;
        }
    }
    
    // 👇 新增解析方法
    private static void ParseBulletRecipes(GameConfig config, TextAsset csv)
    {
        string[] lines = csv.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            if (cols.Length < 10) continue; // 我们有10列

            BulletData data = new BulletData
            {
                Id = cols[0].Trim(),
                Speed = float.Parse(cols[1].Trim(), CultureInfo.InvariantCulture),
                Damage = float.Parse(cols[2].Trim(), CultureInfo.InvariantCulture),
                LifeTime = float.Parse(cols[3].Trim(), CultureInfo.InvariantCulture),
                ShootInterval = float.Parse(cols[4].Trim(), CultureInfo.InvariantCulture),
                SlowRatio = float.Parse(cols[5].Trim(), CultureInfo.InvariantCulture),
                SlowDuration = float.Parse(cols[6].Trim(), CultureInfo.InvariantCulture),
                ChainTargets = int.Parse(cols[7].Trim(), CultureInfo.InvariantCulture),
                ChainRange = float.Parse(cols[8].Trim(), CultureInfo.InvariantCulture),
                AOERadius = float.Parse(cols[9].Trim(), CultureInfo.InvariantCulture)
            };
            config.BulletRecipes[data.Id] = data;
        }
    }
}