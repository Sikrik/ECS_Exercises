using System;
using System.Globalization;
using UnityEngine;

/// <summary>
/// 全局配置加载器：负责将所有 CSV 数据反序列化为内存中的 GameConfig 对象
/// </summary>
public static class ConfigLoader
{
    // 主入口
    public static GameConfig Load()
    {
        GameConfig config = new GameConfig();

        TextAsset baseCsv = Resources.Load<TextAsset>("Configs/game_config");
        if (baseCsv != null) ParseBaseSettings(config, baseCsv);
        else Debug.LogError("初始化失败：未找到 Resources/game_config.csv");

        TextAsset playerCsv = Resources.Load<TextAsset>("Configs/Player_config");
        if (playerCsv != null) ParsePlayerRecipes(config, playerCsv);
        else Debug.LogError("初始化失败：未找到 Resources/Player_config.csv");

        TextAsset enemyCsv = Resources.Load<TextAsset>("Configs/Enemy_config");
        if (enemyCsv != null) ParseEnemyRecipes(config, enemyCsv);
        else Debug.LogError("初始化失败：未找到 Resources/Enemy_config.csv");

        TextAsset waveCsv = Resources.Load<TextAsset>("Configs/Wave_config");
        if (waveCsv != null) ParseWaveSettings(config, waveCsv);
        else Debug.LogError("初始化失败：未找到 Resources/Wave_config.csv");

        TextAsset upgradeCsv = Resources.Load<TextAsset>("Configs/Upgrade_Config");
        if (upgradeCsv != null) ParseUpgradeRecipes(config, upgradeCsv);
        else Debug.LogError("初始化失败：未找到 Resources/Upgrade_Config.csv");

        TextAsset levelCsv = Resources.Load<TextAsset>("Configs/Level_Config");
        if (levelCsv != null) ParseLevelRecipes(config, levelCsv);
        else Debug.LogError("初始化失败：未找到 Resources/Level_Config.csv");

        return config;
    }

    private static void ParseBaseSettings(GameConfig config, TextAsset csv)
    {
        string[] lines = csv.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            if (cols.Length < 2) continue;

            string key = cols[0].Trim();
            string valueStr = cols[1].Trim();

            switch (key)
            {
                case "CollisionPushDistance": config.CollisionPushDistance = ParseFloat(valueStr); break;
                case "CollisionBounceForce": config.CollisionBounceForce = ParseFloat(valueStr); break;
                case "InitialSpawnInterval": config.InitialSpawnInterval = ParseFloat(valueStr); break;
                // 全局成长系数已废弃，可在此保留以防 game_config.csv 未删除报错
                case "EnemyHpGrowth": config.EnemyHpGrowth = ParseFloat(valueStr); break;
                case "EnemyDmgGrowth": config.EnemyDmgGrowth = ParseFloat(valueStr); break;
                case "EnemySpeedGrowth": config.EnemySpeedGrowth = ParseFloat(valueStr); break;
            }
        }
    }

    private static void ParsePlayerRecipes(GameConfig config, TextAsset csv)
    {
        string[] lines = csv.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            if (cols.Length < 10) continue; 

            PlayerData data = new PlayerData
            {
                Id = cols[0].Trim(),
                MaxHealth = ParseFloat(cols[1]),
                MoveSpeed = ParseFloat(cols[2]),
                InvincibleDuration = ParseFloat(cols[3]),
                Mass = ParseFloat(cols[4]),
                FireRate = ParseFloat(cols[5]),
                DashSpeed = ParseFloat(cols[6]),
                DashDuration = ParseFloat(cols[7]),
                DashCD = ParseFloat(cols[8]),
                DefaultBullet = cols[9].Trim()
            };
            config.PlayerRecipes[data.Id] = data;
        }
    }

    private static void ParseEnemyRecipes(GameConfig config, TextAsset csv)
    {
        string[] lines = csv.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            if (cols.Length < 8) continue; // 增加了等级列，最少需要8列(到Traits)

            EnemyData data = new EnemyData
            {
                Id = cols[0].Trim(),
                Level = ParseInt(cols[1]), // 读取等级
                Health = ParseFloat(cols[2]),
                Speed = ParseFloat(cols[3]),
                Damage = ParseInt(cols[4]),
                HitRecoveryDuration = ParseFloat(cols[5]),
                EnemyDeathScore = ParseInt(cols[6]),
                Traits = string.IsNullOrWhiteSpace(cols[7]) ? new string[0] : cols[7].Trim().Split('|'),
                BounceForce = cols.Length > 8 ? ParseFloat(cols[8]) : 5.0f,
                FireRate      = cols.Length > 9 ? ParseFloat(cols[9]) : 0f,
                ActionDist1   = cols.Length > 10 ? ParseFloat(cols[10]) : 0f,
                ActionDist2   = cols.Length > 11 ? ParseFloat(cols[11]) : 0f,
                ActionDist3   = cols.Length > 12 ? ParseFloat(cols[12]) : 0f,
                ActionTime1   = cols.Length > 13 ? ParseFloat(cols[13]) : 0f,
                SkillSpeed    = cols.Length > 14 ? ParseFloat(cols[14]) : 0f,
                SkillDuration = cols.Length > 15 ? ParseFloat(cols[15]) : 0f,
                SkillCD       = cols.Length > 16 ? ParseFloat(cols[16]) : 0f
            };
            
            // 【核心修改】将字典的键改为 ID_Level 的组合键
            string key = $"{data.Id}_{data.Level}";
            config.EnemyRecipes[key] = data;
        }
    }

    private static void ParseWaveSettings(GameConfig config, TextAsset csv)
    {
        string[] lines = csv.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            if (cols.Length < 4) continue; 

            WaveData data = new WaveData
            {
                WaveIndex = ParseInt(cols[0]),
                SpawnInterval = ParseFloat(cols[2]),
                NextWaveDelay = ParseFloat(cols[3])
            };

            string spawnsData = cols[1].Trim();
            if (!string.IsNullOrEmpty(spawnsData))
            {
                string[] spawns = spawnsData.Split('|');
                foreach (var s in spawns)
                {
                    string[] kv = s.Split(':');
                    if (kv.Length == 3)
                    {
                        string eId = kv[0].Trim();
                        int level = ParseInt(kv[1]);
                        int count = ParseInt(kv[2]);
                        data.SpawnList.Add(new EnemySpawnInfo { Id = eId, Level = level, Count = count });
                        data.TotalSpawnCount += count; 
                    }
                }
            }
            config.Waves.Add(data);
        }
    }

    private static void ParseUpgradeRecipes(GameConfig config, TextAsset csv)
    {
        string[] lines = csv.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            if (cols.Length < 3) continue;

            UpgradeData data = new UpgradeData
            {
                Id = cols[0].Trim(),
                MaxLevel = ParseInt(cols[1]),
                Description = cols[2].Trim(),
                // 解析第四列(前置条件)，没有则留空
                Prerequisite = cols.Length > 3 ? cols[3].Trim() : string.Empty 
            };
            config.UpgradeRecipes[data.Id] = data;
        }
    }

    private static void ParseLevelRecipes(GameConfig config, TextAsset csv)
    {
        string[] lines = csv.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            if (cols.Length < 2) continue;
            config.LevelExpRecipes[ParseInt(cols[0])] = ParseInt(cols[1]);
        }
    }

    private static float ParseFloat(string s)
    {
        if (float.TryParse(s.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out float result))
            return result;
        return 0f;
    }

    private static int ParseInt(string s)
    {
        if (int.TryParse(s.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out int result))
            return result;
        return 0;
    }
}