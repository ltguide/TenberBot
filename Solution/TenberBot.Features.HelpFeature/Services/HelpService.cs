using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using TenberBot.Features.HelpFeature.Data.POCO;
using TenberBot.Shared.Features.Attributes.Modules;
using TenberBot.Shared.Features.Data.POCO;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;
using TenberBot.Shared.Features.Services;
using TenberBot.Shared.Features.Settings.Server;

namespace TenberBot.Features.HelpFeature.Services;

public interface IHelpService
{
    string Description { get; }

    IList<EmbedFieldBuilder> GetFields(IList<HelpCommandInfo> commands);

    Task<MessageProperties> BuildMessage(SocketCommandContext context, int currentPage);
}

public class HelpService : IHelpService
{
    public string Description => "An option surrounded with `[]` means it is **not** required.\nAn option surrounded with `<>` is *usually* required.\nA `|` symbol means provide one of the values if applicable.\nNote: slash commands may appear that you cannot access.";

    private readonly IServiceProvider serviceProvider;
    private readonly InteractionService interactionService;
    private readonly CommandService commandService;
    private readonly CacheService cacheService;

    public HelpService(
        IServiceProvider serviceProvider,
        InteractionService interactionService,
        CommandService commandService,
        CacheService cacheService)
    {
        this.serviceProvider = serviceProvider;
        this.interactionService = interactionService;
        this.commandService = commandService;
        this.cacheService = cacheService;
    }

    public IList<EmbedFieldBuilder> GetFields(IList<HelpCommandInfo> commands)
    {
        var fields = new List<EmbedFieldBuilder>();

        foreach (var grouping in commands.GroupBy(x => x.Group))
        {
            fields.Add(new EmbedFieldBuilder { Name = "\u200B", Value = $"__**{grouping.Key}**__", });

            foreach (var command in grouping)
            {
                var additionally = "";
                if (command.Aliases?.Length > 0)
                    additionally = $"\n*Alias{(command.Aliases.Length != 1 ? "es" : "")}*: `{string.Join("`, `", command.Aliases)}`";

                fields.Add(new EmbedFieldBuilder { Name = $"`{command.Name}` {command.Arguments}", Value = $"> {command.Description}{additionally}", });
            }
        }

        return fields;
    }

    public async Task<MessageProperties> BuildMessage(SocketCommandContext context, int currentPage)
    {
        var prefix = cacheService.Get<BasicServerSettings>(context.Guild).Prefix;

        var commands = (await commandService.GetExecutableCommandsAsync(context, serviceProvider)).Where(x => x.Summary != null).Select(x => new HelpCommandInfo(prefix, x))
            .Concat(GetDefaultSlashCommands(context.User))
            .ToList();

        var view = new PageView
        {
            PerPage = 5,
            CurrentPage = currentPage,
        };
        view.PageCount = view.CalcPages(commands.Count);

        commands = commands
            .OrderBy(x => x.Group)
            .ThenBy(x => x.Name)
            .Skip(view.CurrentPage * view.PerPage)
            .Take(view.PerPage)
            .ToList();

        return new MessageProperties
        {
            Embed = GetEmbed(context, commands, view),
            Components = GetComponents(context, view),
        };
    }

    private List<HelpCommandInfo> GetDefaultSlashCommands(SocketUser socketUser)
    {
        var commands = new List<HelpCommandInfo>();

        if (socketUser is not SocketGuildUser socketGuildUser)
            return commands;

        foreach (var command in interactionService.SlashCommands)
        {
            if (command.Attributes.Any(x => x is HelpCommandAttribute) == false)
                continue;

            if (command.Module.DefaultMemberPermissions != null && socketGuildUser.GuildPermissions.Has(command.Module.DefaultMemberPermissions.Value) == false)
                continue;

            if (command.DefaultMemberPermissions != null && socketGuildUser.GuildPermissions.Has(command.DefaultMemberPermissions.Value) == false)
                continue;

            commands.Add(new HelpCommandInfo(command));
        }

        return commands;
    }

    private Embed GetEmbed(SocketCommandContext context, IList<HelpCommandInfo> commands, PageView view)
    {
        var embedBuilder = new EmbedBuilder
        {
            Author = context.User.GetEmbedAuthor("'s Available Commands"),
            Description = Description,
        };

        if (commands.Count == 0)
            embedBuilder.Description += "\n*No results found.*";
        else
            embedBuilder
                .WithFields(GetFields(commands))
                .WithFooter($"Page {view.CurrentPage + 1} of {view.PageCount + 1}");

        return embedBuilder.Build();
    }

    private static MessageComponent GetComponents(SocketCommandContext context, PageView view)
    {
        var componentBuilder = new ComponentBuilder()
            .WithButton(customId: $"help-page:{context.User.Id},first,0", emote: new Emoji("⏮"))
            .WithButton(customId: $"help-page:{context.User.Id},previous,{Math.Max(0, view.CurrentPage - 1)}", emote: new Emoji("⏪"))
            .WithButton(customId: $"help-page:{context.User.Id},next,{Math.Max(0, Math.Min(view.PageCount, view.CurrentPage + 1))}", emote: new Emoji("⏩"))
            .WithButton(customId: $"help-page:{context.User.Id},last,{Math.Max(0, view.PageCount)}", emote: new Emoji("⏭"));

        return componentBuilder.Build();
    }
}
