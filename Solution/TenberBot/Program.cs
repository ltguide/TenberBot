using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using NLog.Extensions.Logging;
using TenberBot.Handlers;
using TenberBot.Shared.Features;

namespace TenberBot;

public class Program
{
    public static async Task Main(string[] args)
    {
        var logLevel = LogSeverity.Info;
        //var logLevel = LogSeverity.Verbose;

        var host = Host.CreateDefaultBuilder(args)
            .UseWindowsService(options =>
            {
                options.ServiceName = "TenberBot";
            })
            .ConfigureAppConfiguration(config =>
            {
                config.AddEnvironmentVariables("TenberBot-");
            })
            .ConfigureDiscordHost((context, config) =>
            {
                config.SocketConfig = new DiscordSocketConfig
                {
                    LogLevel = logLevel,
                    MessageCacheSize = 200,
                    AlwaysDownloadUsers = true,
                    GatewayIntents = (GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers) & ~GatewayIntents.GuildScheduledEvents & ~GatewayIntents.GuildInvites,
                    UseInteractionSnowflakeDate = false,
                };

                config.Token = context.Configuration["app-token"];
            })
            .UseCommandService((context, config) =>
            {
                config.LogLevel = logLevel;
                config.DefaultRunMode = RunMode.Async;
                config.CaseSensitiveCommands = false;
            })
            .UseInteractionService((context, config) =>
            {
                config.LogLevel = logLevel;
                config.UseCompiledLambda = true;
                config.InteractionCustomIdDelimiters = new[] { ' ', };
            })
            .ConfigureLogging((host, config) =>
            {
                config.ClearProviders();
                config.SetMinimumLevel(LogLevel.Trace);
                config.AddNLog(host.Configuration);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddMemoryCache();

                services.AddHostedService<GuildMessageHandler>();
                services.AddHostedService<InteractionHandler>();

                SharedFeatures.RegisterFeatures(services, AppContext.BaseDirectory);
            })
            .Build();

        //await host.Services.GetRequiredService<DataContext>()
        //    .Database.MigrateAsync()
        //    .ConfigureAwait(false);

        await host.RunAsync();
    }
}



// Embed markdown support
//  Title (no mentions)
//  Description
//  Field name (no mentions)
//  Field value



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