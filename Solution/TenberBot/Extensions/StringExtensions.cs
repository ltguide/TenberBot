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
}
