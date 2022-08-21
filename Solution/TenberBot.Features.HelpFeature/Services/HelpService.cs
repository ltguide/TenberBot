using Discord;
using Discord.Commands;
using TenberBot.Shared.Features.Data.POCO;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;
using TenberBot.Shared.Features.Extensions.Strings;
using TenberBot.Shared.Features.Services;
using TenberBot.Shared.Features.Settings.Server;

namespace TenberBot.Features.HelpFeature.Services;

public interface IHelpService
{
    string Description { get; }

    IList<EmbedFieldBuilder> GetFields(string prefix, IList<CommandInfo> commands);

    Task<MessageProperties> BuildMessage(SocketCommandContext context, int currentPage);
}

public class HelpService : IHelpService
{
    public string Description => "An option surrounded with `[]` means it is **not** required.\nAn option surrounded with `<>` is *usually* required.\nA `|` symbol means provide one of the values if applicable.";

    private readonly IServiceProvider serviceProvider;
    private readonly CommandService commandService;
    private readonly CacheService cacheService;

    public HelpService(
        IServiceProvider serviceProvider,
        CommandService commandService,
        CacheService cacheService)
    {
        this.serviceProvider = serviceProvider;
        this.commandService = commandService;
        this.cacheService = cacheService;
    }

    public IList<EmbedFieldBuilder> GetFields(string prefix, IList<CommandInfo> commands)
    {
        var fields = new List<EmbedFieldBuilder>();

        foreach (var grouping in commands.GroupBy(x => x.Module.Remarks))
        {
            fields.Add(new EmbedFieldBuilder { Name = "\u200B", Value = $"__**{grouping.Key ?? "General"}**__", });

            foreach (var command in grouping)
            {
                var aliases = command.Aliases.Skip(1).ToList();

                var additionally = "";
                if (aliases.Count > 0)
                    additionally = $"\n*Alias{(aliases.Count != 1 ? "es" : "")}*: `{prefix}{string.Join($"`, `{prefix}", aliases)}`";

                fields.Add(new EmbedFieldBuilder { Name = $"`{prefix}{command.Aliases[0]}` {command.Remarks}", Value = $"> {command.Summary}{additionally}", });
            }
        }

        return fields;
    }

    public async Task<MessageProperties> BuildMessage(SocketCommandContext context, int currentPage)
    {
        var commands = (await commandService.GetExecutableCommandsAsync(context, serviceProvider)).Where(x => x.Summary != null).ToList();

        var view = new PageView
        {
            PerPage = 5,
            CurrentPage = currentPage,
        };
        view.PageCount = view.CalcPages(commands.Count);

        commands = commands
            .OrderBy(x => x.Module.Remarks)
            .ThenBy(x => x.Aliases[0])
            .Skip(view.CurrentPage * view.PerPage)
            .Take(view.PerPage)
            .ToList();

        return new MessageProperties
        {
            Embed = GetEmbed(context, commands, view),
            Components = GetComponents(context, view),
        };
    }

    private Embed GetEmbed(SocketCommandContext context, IList<CommandInfo> commands, PageView view)
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
                .WithFields(GetFields(cacheService.Get<BasicServerSettings>(context.Guild).Prefix.SanitizeMD(), commands))
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
