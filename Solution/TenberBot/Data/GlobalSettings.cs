using Discord;

namespace TenberBot.Data;

public static class GlobalSettings
{
    public static string Prefix { get; set; } = "!";

    public static IEmote EmoteSuccess { get; set; } = new Emoji("✔");

    public static IEmote EmoteFail { get; set; } = new Emoji("❌");

    public static IEmote EmoteUnknown { get; set; } = new Emoji("❔");
}
