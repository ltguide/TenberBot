using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using TenberBot.Shared.Features;
using TenberBot.Shared.Features.Extensions.DiscordRoot;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;
using TenberBot.Shared.Features.Extensions.Strings;
using TenberBot.Shared.Features.Services;
using TenberBot.Shared.Features.Settings.Server;

namespace TenberBot.Features.HelpFeature.Modules.Command;

[Remarks("Information")]
public class InformationCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly IServiceProvider serviceProvider;
    private readonly CommandService commandService;
    private readonly DiscordSocketClient client;
    private readonly CacheService cacheService;

    public InformationCommandModule(
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
            .WithAuthor("Commands for Everyone", client.CurrentUser.GetCurrentAvatarUrl());

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

    [Command("debug-version", ignoreExtraArgs: true)]
    public async Task ShowVersion()
    {
        var assembly = Assembly.GetEntryAssembly();
        if (assembly == null)
            throw new InvalidOperationException();

        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "??";

        await Context.Message.ReplyAsync($"**{assembly.GetName().Name}** {version}\n\n*Modules:*\n> {string.Join("\n> ", SharedFeatures.Assemblies.Select(x=> x.GetName().Name))}");
    }

    [Command("debug-roles", ignoreExtraArgs: true)]
    public async Task ShowRoles()
    {
        if (Context.User is not SocketGuildUser user)
            return;

        await Context.Message.ReplyAsync($"I think you have these roles: {string.Join(", ", user.Roles.OrderByDescending(x => x.Position).Select(x => x.Mention))}", allowedMentions: AllowedMentions.None);
    }

    [Command("debug-latency", ignoreExtraArgs: true)]
    public async Task ShowLatency()
    {
        await Context.Message.ReplyAsync($"Most recent latency: {client.Latency}ms");
    }


    [Command("debug-avatar", ignoreExtraArgs: true)]
    public async Task ShowAvatars()
    {
        await Context.Message.ReplyAsync($"Out of the available, using this one: <{Context.User.GetCurrentAvatarUrl()}>\n> GetGuildAvatarUrl: {(Context.User as SocketGuildUser)?.GetGuildAvatarUrl()}\n> GetAvatarUrl: {Context.User.GetAvatarUrl()}\n> GetDefaultAvatarUrl: {Context.User.GetDefaultAvatarUrl()}");
    }

    [Command("debug-react", ignoreExtraArgs: true)]
    public async Task AddReaction()
    {
        var emotes = cacheService.Get<EmoteServerSettings>(Context.Guild);

        _ = Task.Run(async () =>
        {
            await Context.Message.AddReactionsAsync(new[] {
                emotes.Success,
                emotes.Fail,
                emotes.Busy,
            });
        });

        await ReplyAsync($"{emotes.Success.ToString()!.SanitizeMD()} / {emotes.Fail.ToString()!.SanitizeMD()} / {emotes.Busy.ToString()!.SanitizeMD()}");

        await ReplyAsync($"{emotes.Success} / {emotes.Fail} / {emotes.Busy}");
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
