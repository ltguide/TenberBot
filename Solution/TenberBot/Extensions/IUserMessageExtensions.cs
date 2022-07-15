using Discord;
using System.Text.RegularExpressions;

namespace TenberBot.Extensions;

public static class IUserMessageExtensions
{
    public static void DeleteSoon(this IUserMessage message, TimeSpan? timeSpan = null)
    {
        _ = Task.Delay(timeSpan ?? TimeSpan.FromSeconds(5))
            .ContinueWith(_ => message.DeleteAsync());
    }

    public static bool HasInnerAlias(this IUserMessage message, string prefix, IList<string> aliases, ref int argPos)
    {
        foreach (Match match in Regex.Matches(message.Content, @$" {Regex.Escape(prefix)}(\S+)", RegexOptions.IgnoreCase))
            if (aliases.Contains(match.Groups[1].Value))
            {
                argPos = match.Index + 2;
                return true;
            }

        return false;
    }
}
