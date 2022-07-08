using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;
using TenberBot.Parameters;

namespace TenberBot.Handlers;

// NOTE: This command handler is specifically for using InteractionService-based commands
internal class InteractionHandler : DiscordClientService
{
    private readonly IServiceProvider provider;
    private readonly InteractionService interactionService;
    private readonly IHostEnvironment environment;
    private readonly IConfiguration configuration;

    public InteractionHandler(DiscordSocketClient client, ILogger<DiscordClientService> logger, IServiceProvider provider, InteractionService interactionService, IHostEnvironment environment, IConfiguration configuration) : base(client, logger)
    {
        this.provider = provider;
        this.interactionService = interactionService;
        this.environment = environment;
        this.configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Process the InteractionCreated payloads to execute Interactions commands
        Client.InteractionCreated += HandleInteraction;

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

    private async Task HandleInteraction(SocketInteraction arg)
    {
        try
        {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
            var ctx = new SocketInteractionContext(Client, arg);
            await interactionService.ExecuteCommandAsync(ctx, provider);
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
}
