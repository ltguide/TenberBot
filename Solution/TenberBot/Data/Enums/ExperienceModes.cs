namespace TenberBot.Data.Enums;

[Flags]
public enum ExperienceModes
{
    Disabled = 0,
    Normal = 1 << 0,
    Event = 1 << 1,
}
