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

    public static bool HasInnerAlias(this IUserMessage message, string prefix, IList<string> aliases, out string command)
    {
        foreach (Match match in Regex.Matches(message.Content, @$" {Regex.Escape(prefix)}([-\w]+)", RegexOptions.IgnoreCase))
        {
            command = match.Groups[1].Value.ToLower();
            if (aliases.Contains(command))
                return true;
        }

        command = "";
        return false;
    }
}
