using Discord;

namespace TenberBot.Data;

public static class ServerSettings
{
    public static string Prefix => "prefix";
    public static string EmoteSuccess => "emote-success";
    public static string EmoteFail => "emote-fail";
    public static string EmoteBusy => "emote-busy";


    public static IReadOnlyDictionary<string, object> Defaults = new Dictionary<string, object>
    {
        { Prefix, "!" },
        { EmoteSuccess, new Emoji("✔") },
        { EmoteFail, new Emoji("❌") },
        { EmoteBusy, new Emoji("❔") },
    };
}
