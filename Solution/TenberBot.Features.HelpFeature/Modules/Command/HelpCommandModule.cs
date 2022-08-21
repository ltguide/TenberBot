using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TenberBot.Features.HelpFeature.Services;
using TenberBot.Shared.Features.Extensions.DiscordRoot;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;
using TenberBot.Shared.Features.Extensions.Strings;
using TenberBot.Shared.Features.Services;
using TenberBot.Shared.Features.Settings.Server;

namespace TenberBot.Features.HelpFeature.Modules.Command;

[Remarks("Information")]
public class HelpCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly IHelpService helpService;
    private readonly IServiceProvider serviceProvider;
    private readonly CommandService commandService;
    private readonly DiscordSocketClient client;
    private readonly CacheService cacheService;

    public HelpCommandModule(
        IHelpService helpService,
        IServiceProvider serviceProvider,
        CommandService commandService,
        DiscordSocketClient client,
        CacheService cacheService)
    {
        this.helpService = helpService;
        this.serviceProvider = serviceProvider;
        this.commandService = commandService;
        this.client = client;
        this.cacheService = cacheService;
    }

    [Command("help", ignoreExtraArgs: true)]
    public async Task Help()
    {
        var messageProperties = await helpService.BuildMessage(Context, 0);

        await Context.Message.ReplyAsync(embed: messageProperties.Embed.Value, components: messageProperties.Components.Value);
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

        var prefix = cacheService.Get<BasicServerSettings>(Context.Guild).Prefix.SanitizeMD();
        var embeds = helpService.GetFields(prefix, commands).Chunk(25).Select((x, i) =>
        {
            var embedBuilder = new EmbedBuilder();

            embedBuilder.WithFields(x);

            if (i == 0)
                embedBuilder
                    .WithDescription(helpService.Description)
                    .WithAuthor("Commands for Everyone", client.CurrentUser.GetCurrentAvatarUrl());

            return embedBuilder.Build();
        }).ToArray();

        await ReplyAsync(embeds: embeds);

        Context.Message.DeleteSoon();
    }
}
