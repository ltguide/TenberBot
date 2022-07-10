using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;
using TenberBot.Parameters;
using TenberBot.Services;

namespace TenberBot.Handlers;

// NOTE: This command handler is specifically for using InteractionService-based commands
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
        //_interactionService.SlashCommandExecuted += SlashCommandExecuted;
        //_interactionService.ContextCommandExecuted += ContextCommandExecuted;
        //_interactionService.ComponentCommandExecuted += ComponentCommandExecuted;

        interactionService.AddTypeConverter<TimeSpan>(new TimeSpanConverter());

        await interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), provider);

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
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
            var context = new SocketInteractionContext(Client, arg);

            await cacheService.Channel(context.Channel);

            await interactionService.ExecuteCommandAsync(context, provider);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception occurred whilst attempting to handle interaction.");

            if (arg.Type == InteractionType.ApplicationCommand)
            {
                var msg = await arg.GetOriginalResponseAsync();
                await msg.DeleteAsync();
            }
        }
    }

    //private Task ComponentCommandExecuted(ComponentCommandInfo commandInfo, IInteractionContext context, IResult result)
    //{
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

    //    return Task.CompletedTask;
    //}

    //private Task ContextCommandExecuted(ContextCommandInfo context, IInteractionContext arg2, IResult result)
    //{
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

    //    return Task.CompletedTask;
    //}

    //private Task SlashCommandExecuted(SlashCommandInfo commandInfo, IInteractionContext context, IResult result)
    //{
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

    //    return Task.CompletedTask;
    //}
}
