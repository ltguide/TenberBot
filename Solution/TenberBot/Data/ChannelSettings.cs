namespace TenberBot.Data;

public static class ChannelSettings
{
    public static string SprintMode => "sprint-mode";
    public static string SprintRole => "sprint-role";

    public static string ExperienceEnabled => "experience-enabled";
    public static string ExperienceMessage => "experience-message";
    public static string ExperienceMessageLine => "experience-message-line";
    public static string ExperienceMessageWord => "experience-message-word";
    public static string ExperienceMessageCharacter => "experience-message-character";
    public static string ExperienceMessageAttachment => "experience-message-attachment";
    public static string ExperienceVoiceMinute => "experience-voice-minute";


    public static IReadOnlyDictionary<string, object> Defaults = new Dictionary<string, object>
    {
        { SprintMode, Enums.SprintMode.Disabled },
        { SprintRole, "" },
        { ExperienceEnabled, true },
        { ExperienceMessage, 1m },
        { ExperienceMessageLine, 0.1m },
        { ExperienceMessageWord, 0.05m },
        { ExperienceMessageCharacter, 0.01m },
        { ExperienceMessageAttachment, 0.1m },
        { ExperienceVoiceMinute, 1m },
    };
}
