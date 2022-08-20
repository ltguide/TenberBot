using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.VisualBasic;
using TenberBot.Shared.Features.Extensions.DiscordRoot;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;
using TenberBot.Shared.Features.Extensions.Strings;
using TenberBot.Shared.Features.Services;
using TenberBot.Shared.Features.Settings.Server;

namespace TenberBot.Features.HelpFeature.Modules.Command;

[Remarks("Information")]
public class HelpCommandModule : ModuleBase<SocketCommandContext>
{
    private const string description = "An option surrounded with `[]` means it is **not** required.\nAn option surrounded with `<>` is *usually* required.\nA `|` symbol means provide one of the values if applicable.";

    private readonly IServiceProvider serviceProvider;
    private readonly CommandService commandService;
    private readonly DiscordSocketClient client;
    private readonly CacheService cacheService;

    public HelpCommandModule(
        IServiceProvider serviceProvider,
        CommandService commandService,
        DiscordSocketClient client,
        CacheService cacheService)
    {
        this.serviceProvider = serviceProvider;
        this.commandService = commandService;
        this.client = client;
        this.cacheService = cacheService;
    }

    [Command("help", ignoreExtraArgs: true)]
    public async Task Help(int page = 1)
    {
        var embedBuilder = await HelpPage(page, 10, (await commandService.GetExecutableCommandsAsync(Context, serviceProvider)).Where(x => x.Summary != null).ToList());

        if (embedBuilder == null)
            return;

        embedBuilder
            .WithAuthor(Context.User.GetEmbedAuthor("'s Available Commands"));

        await Context.Message.ReplyAsync(embed: embedBuilder.Build()); //, components: new ComponentBuilder().WithButton(customId: "help-page:first,0", emote: new Emoji("⏮")).Build());
    }

    [Command("help-everyone", ignoreExtraArgs: true)]
    [Summary("Show commands that have no permissions.")]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    [RequireBotPermission(ChannelPermission.ManageMessages)]
    public async Task HelpEveryone()
    {
        var commands = new List<CommandInfo>();

        foreach (var command in commandService.Commands.Where(x => x.Summary != null && x.Module.Preconditions.All(x => x is not RequireUserPermissionAttribute) && x.Preconditions.All(x => x is not RequireUserPermissionAttribute)))
        {
            var result = await command.CheckPreconditionsAsync(Context, serviceProvider);

            if (result.IsSuccess)
                commands.Add(command);
        }

        commands = commands
            .OrderBy(x => x.Module.Remarks)
            .ThenBy(x => x.Aliases[0])
            .ToList();

        var embeds = GetFields(commands).Chunk(25).Select((x, i) =>
        {
            var embedBuilder = new EmbedBuilder();

            embedBuilder.WithFields(x);

            if (i == 0)
                embedBuilder
                    .WithDescription(description)
                    .WithAuthor("Commands for Everyone", client.CurrentUser.GetCurrentAvatarUrl());

            return embedBuilder.Build();
        }).ToArray();

        await ReplyAsync(embeds: embeds);

        Context.Message.DeleteSoon();
    }

    private IList<EmbedFieldBuilder> GetFields(List<CommandInfo> commands)
    {
        var prefix = cacheService.Get<BasicServerSettings>(Context.Guild).Prefix.SanitizeMD();

        var fields = new List<EmbedFieldBuilder>();

        foreach (var q in commands.GroupBy(x => x.Module.Remarks))
        {
            fields.Add(new EmbedFieldBuilder { Name = "\u200B", Value = $"__**{q.Key ?? "General"}**__", });

            foreach (var command in q)
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

    [Command("say")]
    [Summary("Echo a message.")]
    [Remarks("`<message>`")]
    [RequireUserPermission(GuildPermission.ManageChannels)]
    public async Task Say([Remainder] string text)
    {
        await Context.Message.ReplyAsync($"{Context.User.GetDisplayNameSanitized()} told me to say: {text}");
    }

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

        var prefix = cacheService.Get<BasicServerSettings>(Context.Guild).Prefix.SanitizeMD();

        var embedBuilder = new EmbedBuilder()
            .WithFooter($"Page {page} of {pages}");

        if (page == 1)
            embedBuilder.WithDescription(description);

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
