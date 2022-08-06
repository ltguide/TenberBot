namespace TenberBot.Shared.Features.Extensions.Mentions;

public static class MentionUlongExtensions
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
