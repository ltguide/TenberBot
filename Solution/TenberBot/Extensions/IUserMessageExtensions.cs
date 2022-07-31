﻿using Discord;
using System.Text.RegularExpressions;

namespace TenberBot.Extensions;

public static class IUserMessageExtensions
{
    public static void DeleteSoon(this IUserMessage message, TimeSpan? timeSpan = null)
    {
        _ = Task.Delay(timeSpan ?? TimeSpan.FromSeconds(5))
            .ContinueWith(_ => message.DeleteAsync());
    }

    public static bool HasInlineCommand(this IUserMessage message, IList<string> aliases, string prefix, out string command)
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

    public static bool HasInlineTriggers(this IUserMessage message, IDictionary<Regex, string> triggers, out IList<string> commands)
    {
        commands = new List<string>();

        foreach (var trigger in triggers)
        {
            foreach (Match match in trigger.Key.Matches(message.Content))
            {
                var groups = match.Groups.Cast<Group>()
                    .Skip(1)
                    .Where(x => x.Value != "")
                    .Select(x => x.Value)
                    .ToList();

                commands.Add($"{trigger.Value} {(groups.Any() ? string.Join(" ", groups) : match.Groups[0].Value)}");
            }
        }

        return commands.Count != 0;
    }
}
