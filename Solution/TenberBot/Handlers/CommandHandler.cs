using System.Reflection;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using TenberBot.Extensions;
using TenberBot.Parameters;

namespace TenberBot.Handlers;

public class CommandHandler : DiscordClientService
{
    private readonly IServiceProvider _provider;
    private readonly CommandService _commandService;
    private readonly IConfiguration _config;

    public CommandHandler(DiscordSocketClient client, ILogger<CommandHandler> logger, IServiceProvider provider, CommandService commandService, IConfiguration config) : base(client, logger)
    {
        _provider = provider;
        _commandService = commandService;
        _config = config;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Client.MessageReceived += HandleMessage;
        //_commandService.CommandExecuted += CommandExecutedAsync;

        _commandService.AddTypeReader<TimeSpan>(new TimeSpanReader(), true);

        await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
    }

    private async Task HandleMessage(SocketMessage incomingMessage)
    {
        if (incomingMessage is not SocketUserMessage message)
            return;

        if (message.Source != MessageSource.User)
            return;

        int argPos = 0;
        if (!message.HasStringPrefix("!", ref argPos) && !message.HasMentionPrefix(Client.CurrentUser, ref argPos))
            return;

        var context = new SocketCommandContext(Client, message);
        var result = await _commandService.ExecuteAsync(context, argPos, _provider);

        if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
        {
            await message.AddReactionAsync(Program.EmoteFail);
            await message.Channel.SendMessageAsync(result.ErrorReason, messageReference: message.GetReferenceTo());
        }

        // DM:
        //  Author is SocketGlobalUser - Username, Discriminator
        //  Channel is SocketDMChannel

        // Text:
        //  Author is SocketGuildUser - DisplayName, Username, Discriminator, Mention, Guild
        //  Channel is SocketTextChannel - Name, IsNsfw, Mention, Guild

        // Voice:
        //  Author is SocketGuildUser
        //  Channel is SocketVoiceChannel

        // Thread:
        //  Author is SocketGuildUser
        //  Channel is SocketThreadChannel - Name, Owner, ParentChannel
    }

    //public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
    //{
    //    Logger.LogInformation("User {user} attempted to use command {command}", context.User, command.Value.Name);

    //    if (!command.IsSpecified || result.IsSuccess)
    //        return;

    //    await context.Channel.SendMessageAsync($"Error: {result}");
    //}
}