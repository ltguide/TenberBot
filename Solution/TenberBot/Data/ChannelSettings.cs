namespace TenberBot.Data;

public static class ChannelSettings
{
    public static string SprintMode => "sprint-mode";
    public static string SprintRole => "sprint-role";
    public static string ExperienceRate => "experience-rate";


    public static IReadOnlyDictionary<string, object> Defaults = new Dictionary<string, object>
    {
        { SprintMode, Enums.SprintMode.Disabled },
        { SprintRole, "" },
        { ExperienceRate, 0m },
    };
}
