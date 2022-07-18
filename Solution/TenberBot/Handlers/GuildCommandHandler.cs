using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using TenberBot.Data.Settings.Server;
using TenberBot.Extensions;
using TenberBot.Parameters;
using TenberBot.Results.Command;
using TenberBot.Services;

namespace TenberBot.Handlers;

public class GuildCommandHandler : DiscordClientService
{
    private readonly List<string> InnerAliases = new();
    private readonly IServiceProvider provider;
    private readonly CommandService commandService;
    private readonly CacheService cacheService;

    public GuildCommandHandler(
        IServiceProvider provider,
        CommandService commandService,
        CacheService cacheService,
        DiscordSocketClient client,
        ILogger<GuildCommandHandler> logger) : base(client, logger)
    {
        this.provider = provider;
        this.commandService = commandService;
        this.cacheService = cacheService;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Client.MessageReceived += MessageReceived;

        commandService.CommandExecuted += CommandExecuted;

        commandService.AddTypeReader<TimeSpan>(new TimeSpanReader(), true);

        await commandService.AddModulesAsync(Assembly.GetEntryAssembly(), provider);

        InnerAliases.AddRange(commandService.Modules
            .Where(x => x.Remarks == "Greetings")
            .SelectMany(x => x.Commands.Where(x => x.Summary != null))
            .SelectMany(x => x.Aliases));
    }

    private async Task MessageReceived(SocketMessage incomingMessage)
    {
        if (incomingMessage is not SocketUserMessage message)
            return;

        if (message.Source != MessageSource.User)
            return;

        if (message.Channel is not SocketGuildChannel channel)
            return;

        if (cacheService.TryGetValue<BasicServerSettings>(channel.Guild, out var settings) == false)
            return;

        if (string.IsNullOrWhiteSpace(settings.Prefix))
            return;

        var context = new SocketCommandContext(Client, message);

        await cacheService.Channel(channel);

        int argPos = 0;
        if (message.HasStringPrefix(settings.Prefix, ref argPos) || message.HasMentionPrefix(Client.CurrentUser, ref argPos))
            await commandService.ExecuteAsync(context, argPos, provider);

        else if (message.Content == Client.CurrentUser.Id.GetUserMention())
            await commandService.ExecuteAsync(context, "just-bot-name", provider);

        else if (message.HasInnerAlias(settings.Prefix, InnerAliases, out var command))
            await commandService.ExecuteAsync(context, command, provider);
    }

    public async Task CommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
    {
        if (command.IsSpecified == false)
            return;

        if (result.IsSuccess)
        {
            Logger.LogDebug($"User {context.User.Username}#{context.User.Discriminator} successfully used command: {command.Value.Name}");
            return;
        }

        Logger.LogInformation($"User {context.User.Username}#{context.User.Discriminator} failed to use command: {command.Value.Name}");

        var reply = await context.Message.ReplyAsync(result.ErrorReason);

        await context.Message.AddReactionAsync(cacheService.Get<EmoteServerSettings>(context.Guild).Fail);

        if (result is DeleteResult)
            reply.DeleteSoon(TimeSpan.FromSeconds(15));
    }
}
