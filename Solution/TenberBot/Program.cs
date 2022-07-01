using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using NLog.Extensions.Logging;
using TenberBot.Data;
using TenberBot.Data.Services;
using TenberBot.Handlers;
using TenberBot.Services;

namespace TenberBot;

public class Program
{
    public static async Task Main(string[] args)
    {
        var logLevel = LogSeverity.Info;
        //var logLevel = LogSeverity.Verbose;

        var host = Host.CreateDefaultBuilder(args)
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
            })
            .ConfigureLogging((host, config) =>
            {
                config.ClearProviders();
                config.SetMinimumLevel(LogLevel.Trace);
                config.AddNLog(host.Configuration);
            })
            .ConfigureServices((context, services) =>
            {
                //services.AddSingleton<GlobalSettingService>();
                //services.AddHostedService(provider => provider.GetRequiredService<GlobalSettingService>());

                services.AddHostedService<GlobalSettingService>();

                services.AddHostedService<BotStatusService>();

                services.AddHostedService<CommandHandler>();
                services.AddHostedService<InteractionHandler>();

                services.AddDbContext<DataContext>(builder =>
                    builder.UseSqlServer(context.Configuration["app-database"], options =>
                    {
                        options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    }), ServiceLifetime.Transient, ServiceLifetime.Singleton);

                services.AddTransient<IGlobalSettingDataService, GlobalSettingDataService>();
                services.AddTransient<IBotStatusDataService, BotStatusDataService>();
                services.AddTransient<IGreetingDataService, GreetingDataService>();
            })
            .Build();

        await host.RunAsync();
    }
}



// Embed markdown support
//  Title
//  Description
//  Field name / Field value



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