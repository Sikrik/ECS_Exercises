[System.Serializable]
public class UpgradeData 
{
    public string Id;
    public int MaxLevel;
    public string Description;
    public string Prerequisite; // 新增：前置条件升级项ID，为空代表无前置
}