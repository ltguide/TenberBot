﻿using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using TenberBot.Data;
using TenberBot.Parameters;

namespace TenberBot.Handlers;

public class CommandHandler : DiscordClientService
{
    private readonly IServiceProvider _provider;
    private readonly CommandService _commandService;

    public CommandHandler(DiscordSocketClient client, ILogger<CommandHandler> logger, IServiceProvider provider, CommandService commandService) : base(client, logger)
    {
        _provider = provider;
        _commandService = commandService;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Client.MessageReceived += HandleMessage;
        _commandService.CommandExecuted += CommandExecutedAsync;

        _commandService.AddTypeReader<TimeSpan>(new TimeSpanReader(), true);

        await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
    }

    private async Task HandleMessage(SocketMessage incomingMessage)
    {
        if (incomingMessage is not SocketUserMessage message)
            return;

        if (message.Source != MessageSource.User)
            return;

        if (message.Channel is SocketDMChannel)
            return;

        if (string.IsNullOrWhiteSpace(GlobalSettings.Prefix))
            return;

        int argPos = 0;
        if (!message.HasStringPrefix(GlobalSettings.Prefix, ref argPos) && !message.HasMentionPrefix(Client.CurrentUser, ref argPos))
            return;

        var context = new SocketCommandContext(Client, message);
        var result = await _commandService.ExecuteAsync(context, argPos, _provider);
    }

    public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
    {
        Logger.LogInformation("User {user} attempted to use command {command}", context.User, command.Value.Name);

        if (!command.IsSpecified || result.IsSuccess)
            return;

        await context.Message.ReplyAsync(result.ErrorReason);
        await context.Message.AddReactionAsync(GlobalSettings.EmoteFail);
    }
}
