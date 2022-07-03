using Discord;

namespace TenberBot.Extensions;

public static class UlongExtensions
{
    public static string GetUserMention(this ulong value)
    {
        return $"<@{value}>";
    }

    public static string GetChannelMention(this ulong value)
    {
        return $"<#{value}>";
    }

    public static string GetRoleMention(this ulong value)
    {
        return $"<@&{value}>";
    }
}
