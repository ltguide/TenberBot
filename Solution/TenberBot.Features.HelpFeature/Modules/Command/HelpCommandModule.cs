using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TenberBot.Features.HelpFeature.Data.POCO;
using TenberBot.Features.HelpFeature.Services;
using TenberBot.Shared.Features.Attributes.Modules;
using TenberBot.Shared.Features.Extensions.DiscordRoot;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;
using TenberBot.Shared.Features.Extensions.Strings;
using TenberBot.Shared.Features.Services;
using TenberBot.Shared.Features.Settings.Server;
using InteractionService = Discord.Interactions.InteractionService;

namespace TenberBot.Features.HelpFeature.Modules.Command;

[Remarks("Information")]
public class HelpCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly IHelpService helpService;
    private readonly IServiceProvider serviceProvider;
    private readonly InteractionService interactionService;
    private readonly CommandService commandService;
    private readonly DiscordSocketClient client;
    private readonly CacheService cacheService;

    public HelpCommandModule(
        IHelpService helpService,
        IServiceProvider serviceProvider,
        InteractionService interactionService,
        CommandService commandService,
        DiscordSocketClient client,
        CacheService cacheService)
    {
        this.helpService = helpService;
        this.serviceProvider = serviceProvider;
        this.interactionService = interactionService;
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
        var prefix = cacheService.Get<BasicServerSettings>(Context.Guild).Prefix.SanitizeMD();
        var commands = new List<HelpCommandInfo>();

        foreach (var command in commandService.Commands.Where(x => x.Summary != null && x.Module.Preconditions.All(x => x is not RequireUserPermissionAttribute) && x.Preconditions.All(x => x is not RequireUserPermissionAttribute)))
        {
            var result = await command.CheckPreconditionsAsync(Context, serviceProvider);

            if (result.IsSuccess)
                commands.Add(new HelpCommandInfo(prefix, command));
        }

        foreach (var command in interactionService.SlashCommands.Where(x => x.Attributes.Any(x => x is HelpCommandAttribute) && x.Module.DefaultMemberPermissions == null && x.DefaultMemberPermissions == null))
            commands.Add(new HelpCommandInfo(command));

        commands = commands
            .OrderBy(x => x.Group)
            .ThenBy(x => x.Name)
            .ToList();

        var embeds = helpService.GetFields(commands).Chunk(25).Select((x, i) =>
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
