using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using System.Text.RegularExpressions;
using TenberBot.Parameters;
using TenberBot.Shared.Features;
using TenberBot.Shared.Features.Attributes.Modules;
using TenberBot.Shared.Features.Extensions.DiscordRoot;
using TenberBot.Shared.Features.Extensions.Mentions;
using TenberBot.Shared.Features.Results.Command;
using TenberBot.Shared.Features.Services;
using TenberBot.Shared.Features.Settings.Server;

namespace TenberBot.Handlers;

public class GuildMessageHandler : DiscordClientService
{
    private readonly List<IGuildMessageService> GuildMessageServices = new();
    private readonly Dictionary<Regex, string> InlineTriggers = new();
    private readonly List<string> InlineCommands = new();
    private readonly IServiceProvider provider;
    private readonly CommandService commandService;
    private readonly CacheService cacheService;

    public GuildMessageHandler(
        IServiceProvider provider,
        CommandService commandService,
        CacheService cacheService,
        DiscordSocketClient client,
        ILogger<GuildMessageHandler> logger) : base(client, logger)
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

        foreach (var assembly in SharedFeatures.Assemblies)
            await commandService.AddModulesAsync(assembly, provider);

        InlineCommands.AddRange(commandService.Commands
            .Where(x => x.Attributes.Any(x => x is InlineCommandAttribute))
            .SelectMany(x => x.Aliases)
        );

        foreach (var command in commandService.Commands.Where(x => x.Attributes.Any(x => x is InlineTriggerAttribute)))
        {
            if (command.Attributes.First(x => x is InlineTriggerAttribute) is InlineTriggerAttribute attribute)
                InlineTriggers.Add(attribute.Regex, command.Aliases[0]);
        }

        GuildMessageServices.AddRange(provider.GetServices<IGuildMessageService>());
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

        if (channel is SocketThreadChannel thread)
            await cacheService.Channel(thread.ParentChannel);

        bool checkInline = false;

        int argPos = 0;
        if (message.HasStringPrefix(settings.Prefix, ref argPos) || message.HasMentionPrefix(Client.CurrentUser, ref argPos))
        {
            var result = await commandService.ExecuteAsync(context, argPos, provider);
            checkInline = result is SearchResult sr && sr.Error == CommandError.UnknownCommand;
        }

        else if (message.Content == Client.CurrentUser.Id.GetUserMention())
            await commandService.ExecuteAsync(context, "just-bot-name", provider);

        else
            checkInline = true;

        if (checkInline)
        {
            if (HasInlineCommand(message, InlineCommands, settings.Prefix, out var command))
                await commandService.ExecuteAsync(context, command, provider);

            if (HasInlineTriggers(message, InlineTriggers, out var commands))
                foreach (var inlineCommand in commands)
                    await commandService.ExecuteAsync(context, inlineCommand, provider);

            foreach (var service in GuildMessageServices)
                _ = Task.Run(() => { service.Handle(channel, message); });
        }
    }

    public async Task CommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
    {
        if (command.IsSpecified == false)
            return;

        if (result.IsSuccess)
        {
            Logger.LogDebug($"User {context.User.Username}#{context.User.Discriminator} successfully used command: {command.Value.Module.Group} {command.Value.Name}");
            return;
        }

        Logger.LogInformation($"User {context.User.Username}#{context.User.Discriminator} failed to use command: {command.Value.Module.Group} {command.Value.Name}");

        var reply = await context.Message.ReplyAsync(result.ErrorReason);

        await context.Message.AddReactionAsync(cacheService.Get<EmoteServerSettings>(context.Guild).Fail);

        if (result is DeleteResult)
            reply.DeleteSoon(TimeSpan.FromSeconds(15));
    }

    private static bool HasInlineCommand(IUserMessage message, IList<string> aliases, string prefix, out string command)
    {
        int count = 0;
        foreach (var match in Regex.Matches(message.Content, @$"(?:^| ){Regex.Escape(prefix)}([-\w]+)", RegexOptions.IgnoreCase).Cast<Match>())
        {
            if (match == null)
                break;

            command = match.Groups[1].Value.ToLower();
            if (aliases.Contains(command))
                return true;

            if (++count == 15)
                break;
        }

        command = "";
        return false;
    }

    private static bool HasInlineTriggers(IUserMessage message, IDictionary<Regex, string> triggers, out IList<string> commands)
    {
        commands = new List<string>();

        int count = 0;
        foreach (var trigger in triggers)
        {
            foreach (var match in trigger.Key.Matches(message.Content).Cast<Match>())
            {
                if (match == null)
                    break;

                var groups = match.Groups.Cast<Group>()
                    .Skip(1)
                    .Where(x => x.Value != "")
                    .Select(x => x.Value)
                    .ToList();

                commands.Add($"{trigger.Value} {(groups.Any() ? string.Join(" ", groups) : match.Groups[0].Value)}");

                if (++count == 5)
                    break;
            }
        }

        return commands.Count != 0;
    }
}
