[System.Serializable]
public class TalentData 
{
    public string Id;            // 天赋ID，如 "HealthUp"
    public string Name;          // 显示名称
    public string Description;   // 描述（支持占位符）
    public string TargetField;   // 目标属性：Health, Speed, Attack, FireRate, Exp
    public float ValuePerLevel;  // 每级加成数值
    public int MaxLevel;         // 最高等级
    public int CostBase;         // 基础消耗
    public int CostIncrement;    // 每级增加的消耗
}