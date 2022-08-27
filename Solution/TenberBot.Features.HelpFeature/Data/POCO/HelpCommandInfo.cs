using Discord.Commands;
using Discord.Interactions;
using TenberBot.Shared.Features.Attributes.Modules;

namespace TenberBot.Features.HelpFeature.Data.POCO;
public record HelpCommandInfo(string Group, string Name, string Arguments, string Description, string[]? Aliases)
{
    public HelpCommandInfo(SlashCommandInfo x) : this("General", x.Name, "", x.Description, null)
    {
        if (x.Module.Attributes.FirstOrDefault(x => x is HelpCommandAttribute) is HelpCommandAttribute attribute)
            Group = attribute.Group;

        if (x.Module.SlashGroupName != null)
            Name = $"/{x.Module.SlashGroupName} {Name}";
        else
            Name = "/" + Name;

        attribute = (HelpCommandAttribute)x.Attributes.First(x => x is HelpCommandAttribute);

        Arguments = attribute.Arguments;

        if (attribute.Description != "")
            Description = attribute.Description;
    }

    public HelpCommandInfo(string prefix, CommandInfo x) : this(x.Module.Remarks ?? "General", prefix + x.Aliases[0], x.Remarks, x.Summary, x.Aliases.Skip(1).Select(x => prefix + x).ToArray())
    {
    }
}
