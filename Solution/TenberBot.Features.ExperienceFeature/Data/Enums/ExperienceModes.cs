namespace TenberBot.Features.ExperienceFeature.Data.Enums;

[Flags]
public enum ExperienceModes
{
    Disabled = 0,
    Normal = 1 << 0,
    EventA = 1 << 1,
    EventB = 1 << 2,
}
