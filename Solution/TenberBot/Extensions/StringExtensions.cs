using Discord;

namespace TenberBot.Extensions;

public static class StringExtensions
{
    public static string SanitizeMD(this string value)
    {
        return Format.Sanitize(value);
    }

    public static string GetUserMention(this string value)
    {
        return $"<@{value}>";
    }

    public static string GetChannelMention(this string value)
    {
        return $"<#{value}>";
    }

    public static string GetRoleMention(this string value)
    {
        return $"<@&{value}>";
    }

    public static IEmote? AsIEmote(this string value)
    {
        if (Emote.TryParse(value, out var emote))
            return emote;

        if (Emoji.TryParse(value, out var emoji))
            return emoji;

        return null;
    }
}
