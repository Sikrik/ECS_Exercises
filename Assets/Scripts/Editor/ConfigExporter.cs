using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using System.Text;

public class ConfigExporter
{
    [MenuItem("Tools/Export Config to CSV")]
    public static void ExportToCSV()
    {
        // 1. 获取当前的配置数据（假设已经加载到 ECSManager 中）
        GameConfig config = ECSManager.Instance.Config; 
        if (config == null) return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Key,Value,Description"); // 表头

        // 2. 利用反射遍历 GameConfig 的所有公有字段
        FieldInfo[] fields = typeof(GameConfig).GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (var field in fields)
        {
            string name = field.Name;
            object value = field.GetValue(config);
            sb.AppendLine($"{name},{value},"); // 写入 Key 和 Value
        }

        // 3. 保存为 CSV 文件
        string path = Path.Combine(Application.dataPath, "Resources/game_config.csv");
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);

        AssetDatabase.Refresh();
        Debug.Log($"配置已导出至: {path}");
    }
}