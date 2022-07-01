using Discord;

namespace TenberBot.Extensions;

public static class StringExtensions
{
    public static string SanitizeMD(this string value)
    {
        return Format.Sanitize(value);
    }
}
