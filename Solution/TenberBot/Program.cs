using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using NLog.Extensions.Logging;
using System.Text.Json;
using TenberBot.Converters;
using TenberBot.Data;
using TenberBot.Data.Services;
using TenberBot.Handlers;
using TenberBot.Services;

namespace TenberBot;

public class Program
{
    public static readonly DateTime BaseDuration = new(9999, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    public static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Converters = { new IEmoteJsonConverter(), }
    };

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
                services.AddMemoryCache();

                services.AddHostedService<BotStatusService>();

                services.AddSingleton<CacheService>();
                services.AddHostedService(provider => provider.GetRequiredService<CacheService>());

                services.AddSingleton<SprintService>();
                services.AddHostedService(provider => provider.GetRequiredService<SprintService>());

                services.AddSingleton<UserTimerService>();
                services.AddHostedService(provider => provider.GetRequiredService<UserTimerService>());

                services.AddHostedService<GuildCommandHandler>();
                services.AddHostedService<GuildExperienceHandler>();

                services.AddHostedService<InteractionHandler>();

                services.AddDbContext<DataContext>(builder =>
                    builder.UseSqlServer(context.Configuration["app-database"], options =>
                    {
                        options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    }), ServiceLifetime.Transient, ServiceLifetime.Singleton);

                services.AddTransient<IServerSettingDataService, ServerSettingDataService>();
                services.AddTransient<IChannelSettingDataService, ChannelSettingDataService>();

                services.AddTransient<IVisualDataService, VisualDataService>();

                services.AddTransient<IInteractionParentDataService, InteractionParentDataService>();

                services.AddTransient<IBotStatusDataService, BotStatusDataService>();
                services.AddTransient<IGreetingDataService, GreetingDataService>();
                services.AddTransient<IHugDataService, HugDataService>();
                services.AddTransient<IPatDataService, PatDataService>();
                services.AddTransient<IHighFiveDataService, HighFiveDataService>();
                services.AddTransient<ISprintSnippetDataService, SprintSnippetDataService>();
                services.AddTransient<IRankCardDataService, RankCardDataService>();

                services.AddTransient<ISprintDataService, SprintDataService>();

                services.AddTransient<IUserVoiceChannelDataService, UserVoiceChannelDataService>();
                services.AddTransient<IUserLevelDataService, UserLevelDataService>();
                services.AddTransient<IUserStatDataService, UserStatDataService>();
                services.AddTransient<IUserTimerDataService, UserTimerDataService>();

                services.AddHttpClient<WebService>();
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