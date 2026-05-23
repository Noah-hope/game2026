public enum UpgradeType
{
    AttackDamage,
    SkillDamage,
    MaxHealth,
    MoveSpeed,
    AttackCooldown
}

public class UpgradeOption
{
    public string Title;
    public string Description;
    public UpgradeType Type;

    public UpgradeOption(string title, string description, UpgradeType type)
    {
        Title = title;
        Description = description;
        Type = type;
    }
}
