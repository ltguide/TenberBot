using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TenberBot.Data;
using TenberBot.Extensions;
using TenberBot.Services;

namespace TenberBot.Modules.Command;

[Remarks("Information")]
public class InfoCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly IServiceProvider serviceProvider;
    private readonly CommandService commandService;
    private readonly DiscordSocketClient client;
    private readonly CacheService cacheService;
    private readonly ILogger<InfoCommandModule> logger;

    public InfoCommandModule(
        IServiceProvider serviceProvider,
        CommandService commandService,
        DiscordSocketClient client,
        CacheService cacheService,
        ILogger<InfoCommandModule> logger)
    {
        this.serviceProvider = serviceProvider;
        this.commandService = commandService;
        this.client = client;
        this.cacheService = cacheService;
        this.logger = logger;
    }

    [Command("help", ignoreExtraArgs: true)]
    public async Task Help(int page = 1)
    {

        var embedBuilder = await HelpPage(page, 10, (await commandService.GetExecutableCommandsAsync(Context, serviceProvider)).Where(x => x.Summary != null).ToList());

        if (embedBuilder == null)
            return;

        embedBuilder
            .WithAuthor(Context.User.GetEmbedAuthor("'s Available Commands"));

        await ReplyAsync(embed: embedBuilder.Build());
    }

    [Command("help-everyone", ignoreExtraArgs: true)]
    [Summary("Show commands that have no permissions.")]
    [Remarks("`[page#]`")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    [RequireBotPermission(ChannelPermission.ManageMessages)]
    public async Task HelpEveryone(int page = 1)
    {
        var commands = new List<CommandInfo>();

        foreach (var command in commandService.Commands.Where(x => x.Summary != null && x.Module.Preconditions.All(x => x is not RequireUserPermissionAttribute) && x.Preconditions.All(x => x is not RequireUserPermissionAttribute)))
        {
            var result = await command.CheckPreconditionsAsync(Context, serviceProvider);

            if (result.IsSuccess)
                commands.Add(command);
        }

        var embedBuilder = await HelpPage(page, 25, commands);

        if (embedBuilder == null)
            return;


        embedBuilder
            .WithAuthor("Commands for Everyone", client.GetCurrentAvatarUrl());

        await ReplyAsync(embed: embedBuilder.Build());

        Context.Message.DeleteSoon();
    }

    [Command("say")]
    [Summary("Echo a message.")]
    [Remarks("`<message>`")]
    [RequireUserPermission(GuildPermission.ManageChannels)]
    public async Task Say([Remainder] string text)
    {
        await Context.Message.ReplyAsync($"{Context.User.GetDisplayNameSanitized()} told me to say: {text}");
    }

#if DEBUG
    [Command("react", ignoreExtraArgs: true)]
    public Task AddReaction()
    {
        _ = Task.Run(async () =>
        {
            await Context.Message.AddReactionsAsync(new[] {
                cacheService.Cache.Get<IEmote>(Context.Guild, ServerSettings.EmoteSuccess),
                cacheService.Cache.Get<IEmote>(Context.Guild, ServerSettings.EmoteFail),
                cacheService.Cache.Get<IEmote>(Context.Guild, ServerSettings.EmoteBusy),
            });
        });

        return Task.CompletedTask;
    }
#endif

    private async Task<EmbedBuilder?> HelpPage(int page, int perPage, IList<CommandInfo> commands)
    {
        page = Math.Max(1, page);

        var pages = Math.Ceiling((double)commands.Count / perPage);

        if (page > pages)
        {
            (await Context.Message.ReplyAsync($"Only pages 1 - {pages} will show any commands. 😏")).DeleteSoon();
            Context.Message.DeleteSoon();
            return null;
        }

        commands = commands
            .OrderBy(x => x.Module.Remarks)
            .ThenBy(x => x.Aliases[0])
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToList();

        var prefix = cacheService.Cache.Get<string>(Context.Guild, ServerSettings.Prefix).SanitizeMD();

        var embedBuilder = new EmbedBuilder()
            .WithFooter($"Page {page} of {pages}");

        if (page == 1)
            embedBuilder.WithDescription("An option surrounded with `[]` means it is **not** required.\nAn option surrounded with `<>` is *usually* required.\nA `|` symbol means provide one of the values if applicable.");

        foreach (var q in commands.GroupBy(x => x.Module.Remarks))
        {
            embedBuilder.AddField("\u200B", $"__**{q.Key ?? "General"}**__");

            foreach (var command in q)
            {
                var aliases = command.Aliases.Skip(1).ToList();

                var additionally = "";
                if (aliases.Count > 0)
                    additionally = $"\n*Alias{(aliases.Count != 1 ? "es" : "")}*: `{prefix}{string.Join($"`, `{prefix}", aliases)}`";

                embedBuilder.AddField($"`{prefix}{command.Aliases[0]}` {command.Remarks}", $"> {command.Summary}{additionally}");
            }
        }

        return embedBuilder;
    }
}
