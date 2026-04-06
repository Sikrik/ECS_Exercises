using System;
using System.Globalization;
using System.Reflection;
using UnityEngine;

public static class ConfigLoader
{
    /// <summary>
    /// 从 Resources 加载 CSV 配置文件并填充到 GameConfig 对象中
    /// </summary>
    /// <param name="path">资源路径 (如 "game_config")</param>
    public static GameConfig Load(string path)
    {
        TextAsset csvText = Resources.Load<TextAsset>(path);
        if (csvText == null)
        {
            Debug.LogError($"[ConfigLoader] 找不到配置文件: Resources/{path}");
            return null;
        }

        GameConfig config = new GameConfig();
        string[] lines = csvText.text.Split('\n');
        
        // 获取 GameConfig 的所有字段信息用于反射填充
        FieldInfo[] fields = typeof(GameConfig).GetFields(BindingFlags.Public | BindingFlags.Instance);

        // 从第二行开始解析 (跳过表头)
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] columns = line.Split(',');
            if (columns.Length < 2) continue;

            string key = columns[0].Trim();
            
            // 处理可能的 UTF-8 BOM 字符
            if (i == 1 && key.Length > 0 && key[0] == '\uFEFF')
            {
                key = key.Substring(1);
            }

            string valueStr = columns[1].Trim();

            // 通过反射匹配字段名并赋值
            foreach (var field in fields)
            {
                if (field.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        object val = Convert.ChangeType(valueStr, field.FieldType, CultureInfo.InvariantCulture);
                        field.SetValue(config, val);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[ConfigLoader] 字段 {key} 转换失败: {ex.Message}");
                    }
                    break;
                }
            }
        }

        Debug.Log("[ConfigLoader] 配置文件解析成功");
        return config;
    }
}