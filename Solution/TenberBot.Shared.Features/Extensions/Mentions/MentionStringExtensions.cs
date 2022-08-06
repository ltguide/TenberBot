namespace TenberBot.Shared.Features.Extensions.Mentions;

public static class MentionStringExtensions
{
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
