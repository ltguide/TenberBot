using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.Interactions;
using Discord.WebSocket;
using System.Threading.Channels;
using TenberBot.Parameters;
using TenberBot.Shared.Features;
using TenberBot.Shared.Features.Services;

namespace TenberBot.Handlers;

internal class InteractionHandler : DiscordClientService
{
    private readonly IServiceProvider provider;
    private readonly InteractionService interactionService;
    private readonly IHostEnvironment environment;
    private readonly IConfiguration configuration;
    private readonly CacheService cacheService;

    public InteractionHandler(
        IServiceProvider provider,
        InteractionService interactionService,
        IHostEnvironment environment,
        IConfiguration configuration,
        CacheService cacheService,
        DiscordSocketClient client,
        ILogger<DiscordClientService> logger) : base(client, logger)
    {
        this.provider = provider;
        this.interactionService = interactionService;
        this.environment = environment;
        this.configuration = configuration;
        this.cacheService = cacheService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Process the InteractionCreated payloads to execute Interactions commands
        Client.InteractionCreated += InteractionCreated;

        // Process the command execution results 
        interactionService.SlashCommandExecuted += SlashCommandExecuted;
        interactionService.ComponentCommandExecuted += ComponentCommandExecuted;
        interactionService.ModalCommandExecuted += ModalCommandExecuted;
        //interactionService.ContextCommandExecuted += ContextCommandExecuted;

        interactionService.AddTypeConverter<TimeSpan>(new TimeSpanConverter());

        foreach (var assembly in SharedFeatures.Assemblies)
            await interactionService.AddModulesAsync(assembly, provider);

        await Client.WaitForReadyAsync(stoppingToken);

        // If DOTNET_ENVIRONMENT is set to development, only register the commands to a single guild
        if (environment.IsDevelopment())
            await interactionService.RegisterCommandsToGuildAsync(configuration.GetValue<ulong>("devguild"));
        else
            await interactionService.RegisterCommandsGloballyAsync();
    }

    private async Task InteractionCreated(SocketInteraction arg)
    {
        try
        {
            var context = new SocketInteractionContext(Client, arg);

            await cacheService.Channel(context.Channel);

            if (context.Channel is SocketThreadChannel thread)
                await cacheService.Channel(thread.ParentChannel);

            await interactionService.ExecuteCommandAsync(context, provider);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception occurred whilst attempting to handle interaction.");

            if (arg.Type == InteractionType.ApplicationCommand)
            {
                var msg = await arg.GetOriginalResponseAsync();

                if (msg != null)
                    await msg.DeleteAsync();
            }
        }
    }

    private async Task SlashCommandExecuted(SlashCommandInfo commandInfo, IInteractionContext context, IResult result)
    {
        if (result.IsSuccess)
        {
            Logger.LogDebug($"User {context.User.Username}#{context.User.Discriminator} successfully used slash command: {commandInfo.Module.SlashGroupName} {commandInfo.Name}");
            return;
        }

        Logger.LogInformation($"User {context.User.Username}#{context.User.Discriminator} failed to use slash command: {commandInfo?.Module.SlashGroupName} {commandInfo?.Name} | {result.ErrorReason}");

        await context.Interaction.RespondAsync($"{result.Error}: {result.ErrorReason}", ephemeral: true);
    }

    private async Task ComponentCommandExecuted(ComponentCommandInfo commandInfo, IInteractionContext context, IResult result)
    {
        if (result.IsSuccess)
        {
            Logger.LogDebug($"User {context.User.Username}#{context.User.Discriminator} successfully used component command: {commandInfo.Module.SlashGroupName} {commandInfo.Name}");
            return;
        }

        Logger.LogInformation($"User {context.User.Username}#{context.User.Discriminator} failed to use component command: {result.ErrorReason}");

        await context.Interaction.RespondAsync($"{result.Error}: {result.ErrorReason}", ephemeral: true);

        //    if (!result.IsSuccess)
        //    {
        //        switch (result.Error)
        //        {
        //            case InteractionCommandError.UnmetPrecondition:
        //                // implement
        //                break;
        //            case InteractionCommandError.UnknownCommand:
        //                // implement
        //                break;
        //            case InteractionCommandError.BadArgs:
        //                // implement
        //                break;
        //            case InteractionCommandError.Exception:
        //                // implement
        //                break;
        //            case InteractionCommandError.Unsuccessful:
        //                // implement
        //                break;
        //            default:
        //                break;
        //        }
        //    }
    }

    private async Task ModalCommandExecuted(ModalCommandInfo commandInfo, IInteractionContext context, IResult result)
    {
        if (result.IsSuccess)
        {
            Logger.LogDebug($"User {context.User.Username}#{context.User.Discriminator} successfully used modal command: {commandInfo.Module.SlashGroupName} {commandInfo.Name}");
            return;
        }

        Logger.LogInformation($"User {context.User.Username}#{context.User.Discriminator} failed to use modal command: {result.ErrorReason}");

        await context.Interaction.RespondAsync($"{result.Error}: {result.ErrorReason}", ephemeral: true);
    }

    //private Task ContextCommandExecuted(ContextCommandInfo commandInfo, IInteractionContext context, IResult result)
    //{
    //    if (result.IsSuccess)
    //        Logger.LogDebug($"User {context.User.Username}#{context.User.Discriminator} successfully used context command: {commandInfo.Name}");

    //    else
    //        Logger.LogInformation($"User {context.User.Username}#{context.User.Discriminator} failed to use context command: {commandInfo.Name}");

    //    //    if (!result.IsSuccess)
    //    //    {
    //    //        switch (result.Error)
    //    //        {
    //    //            case InteractionCommandError.UnmetPrecondition:
    //    //                // implement
    //    //                break;
    //    //            case InteractionCommandError.UnknownCommand:
    //    //                // implement
    //    //                break;
    //    //            case InteractionCommandError.BadArgs:
    //    //                // implement
    //    //                break;
    //    //            case InteractionCommandError.Exception:
    //    //                // implement
    //    //                break;
    //    //            case InteractionCommandError.Unsuccessful:
    //    //                // implement
    //    //                break;
    //    //            default:
    //    //                break;
    //    //        }
    //    //    }

    //    return Task.CompletedTask;
    //}
}
