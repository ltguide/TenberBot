﻿using Discord;
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
    }

    private async Task MessageReceived(SocketMessage incomingMessage)
    {
        if (incomingMessage is not SocketUserMessage message)
            return;

        if (message.Source != MessageSource.User)
            return;

        if (message.Channel is not SocketGuildChannel)
            return;

        var context = new SocketCommandContext(Client, message);

        if (cacheService.TryGetValue<BasicServerSettings>(context.Guild, out var settings) == false)
            return;

        if (string.IsNullOrWhiteSpace(settings.Prefix))
            return;

        int argPos = 0;
        if (!message.HasStringPrefix(settings.Prefix, ref argPos) && !message.HasMentionPrefix(Client.CurrentUser, ref argPos))
            return;

        await cacheService.Channel(context.Channel);

        await commandService.ExecuteAsync(context, argPos, provider);
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