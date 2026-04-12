// 路径: Assets/Scripts/ECS/Data/Configs/ConfigLoader.cs
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

        // 1. 加载基础全局配置
        TextAsset baseCsv = Resources.Load<TextAsset>("Configs/game_config");
        if (baseCsv != null) ParseBaseSettings(config, baseCsv);
        else Debug.LogError("初始化失败：未找到 Resources/game_config.csv");

        // 2. 加载玩家职业配方
        TextAsset playerCsv = Resources.Load<TextAsset>("Configs/Player_config");
        if (playerCsv != null) ParsePlayerRecipes(config, playerCsv);
        else Debug.LogError("初始化失败：未找到 Resources/Player_config.csv");

        // 3. 加载敌人装配配方
        TextAsset enemyCsv = Resources.Load<TextAsset>("Configs/Enemy_config");
        if (enemyCsv != null) ParseEnemyRecipes(config, enemyCsv);
        else Debug.LogError("初始化失败：未找到 Resources/Enemy_config.csv");

        // 4. 加载子弹数值配方
        TextAsset bulletCsv = Resources.Load<TextAsset>("Configs/Bullet_Config");
        if (bulletCsv != null) ParseBulletRecipes(config, bulletCsv);
        else Debug.LogError("初始化失败：未找到 Resources/Bullet_Config.csv");

        // 5. 【新增】加载波次混合刷新配置
        TextAsset waveCsv = Resources.Load<TextAsset>("Configs/Wave_config");
        if (waveCsv != null) ParseWaveSettings(config, waveCsv);
        else Debug.LogError("初始化失败：未找到 Resources/Wave_config.csv，请确保已创建该配表。");

        return config;
    }

    private static void ParseBaseSettings(GameConfig config, TextAsset csv)
    {
        string[] lines = csv.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        // 从 i=1 开始，跳过表头 (Key, Value, Description)
        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            if (cols.Length < 2) continue;

            string key = cols[0].Trim();
            string valueStr = cols[1].Trim();

            // 玩家相关的硬编码字段已剔除，仅保留纯粹的全局场景与系统设置
            switch (key)
            {
                case "CollisionPushDistance": config.CollisionPushDistance = ParseFloat(valueStr); break;
                case "CollisionBounceForce": config.CollisionBounceForce = ParseFloat(valueStr); break;
                case "InitialSpawnInterval": config.InitialSpawnInterval = ParseFloat(valueStr); break;
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
            if (cols.Length < 7) continue;

            EnemyData data = new EnemyData
            {
                Id = cols[0].Trim(),
                Health = ParseFloat(cols[1]),
                Speed = ParseFloat(cols[2]),
                Damage = ParseInt(cols[3]),
                HitRecoveryDuration = ParseFloat(cols[4]),
                EnemyDeathScore = ParseInt(cols[5]),
                Traits = string.IsNullOrWhiteSpace(cols[6]) ? new string[0] : cols[6].Trim().Split('|'),
                BounceForce = cols.Length > 7 ? ParseFloat(cols[7]) : 5.0f,
                FireRate      = cols.Length > 8 ? ParseFloat(cols[8]) : 0f,
                ActionDist1   = cols.Length > 9 ? ParseFloat(cols[9]) : 0f,
                ActionDist2   = cols.Length > 10 ? ParseFloat(cols[10]) : 0f,
                ActionDist3   = cols.Length > 11 ? ParseFloat(cols[11]) : 0f,
                ActionTime1   = cols.Length > 12 ? ParseFloat(cols[12]) : 0f,
                SkillSpeed    = cols.Length > 13 ? ParseFloat(cols[13]) : 0f,
                SkillDuration = cols.Length > 14 ? ParseFloat(cols[14]) : 0f,
                SkillCD       = cols.Length > 15 ? ParseFloat(cols[15]) : 0f
            };
            config.EnemyRecipes[data.Id] = data;
        }
    }

    private static void ParseBulletRecipes(GameConfig config, TextAsset csv)
    {
        string[] lines = csv.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            if (cols.Length < 10) continue;

            BulletData data = new BulletData
            {
                Id = cols[0].Trim(),
                Speed = ParseFloat(cols[1]),
                Damage = ParseFloat(cols[2]),
                LifeTime = ParseFloat(cols[3]),
                ShootInterval = ParseFloat(cols[4]),
                SlowRatio = ParseFloat(cols[5]),
                SlowDuration = ParseFloat(cols[6]),
                ChainTargets = ParseInt(cols[7]),
                ChainRange = ParseFloat(cols[8]),
                AOERadius = ParseFloat(cols[9]),
                HitRadius = cols.Length > 10 ? ParseFloat(cols[10]) : 0.2f
            };
            config.BulletRecipes[data.Id] = data;
        }
    }

    // ==========================================
    // 【新增】波次数据解析 (支持混合兵种配置)
    // ==========================================
    private static void ParseWaveSettings(GameConfig config, TextAsset csv)
    {
        string[] lines = csv.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            
            // 确保至少有 4 列数据: WaveIndex, Spawns, SpawnInterval, NextWaveDelay
            if (cols.Length < 4) continue; 

            WaveData data = new WaveData
            {
                WaveIndex = ParseInt(cols[0]),
                SpawnInterval = ParseFloat(cols[2]),
                NextWaveDelay = ParseFloat(cols[3])
            };

            // 解析混合兵种配方 (格式例如 "Normal:10|Fast:5|Ranged:2")
            string spawnsData = cols[1].Trim();
            if (!string.IsNullOrEmpty(spawnsData))
            {
                string[] spawns = spawnsData.Split('|');
                foreach (var s in spawns)
                {
                    string[] kv = s.Split(':');
                    if (kv.Length == 2)
                    {
                        string eId = kv[0].Trim();
                        int count = ParseInt(kv[1]);
                        
                        data.SpawnDict[eId] = count;
                        data.TotalSpawnCount += count; // 自动累加本波怪物总数
                    }
                }
            }
            
            config.Waves.Add(data);
        }
    }

    // ==========================================
    // 数据解析助手 (避免因语言环境差异或填表失误导致的闪退)
    // ==========================================
    
    private static float ParseFloat(string s)
    {
        if (float.TryParse(s.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out float result))
            return result;
        Debug.LogWarning($"[ConfigLoader] 无法将 '{s}' 解析为浮点数，已默认为 0");
        return 0f;
    }

    private static int ParseInt(string s)
    {
        if (int.TryParse(s.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out int result))
            return result;
        Debug.LogWarning($"[ConfigLoader] 无法将 '{s}' 解析为整数，已默认为 0");
        return 0;
    }
}