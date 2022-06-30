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
    public static Emote EmoteSuccess { get; private set; } = Emote.Parse("<:tenber_success:991838580961976440>");
    public static Emote EmoteFail { get; private set; } = Emote.Parse("<:tenber_fail:991838580093767760>");
    public static Emote EmoteUnknown { get; private set; } = Emote.Parse("<:tenber_unknown:991839617487749172>");

    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
             .ConfigureDiscordHost((context, config) =>
             {
                 config.SocketConfig = new DiscordSocketConfig
                 {
                     LogLevel = LogSeverity.Info,
                     MessageCacheSize = 200,
                     AlwaysDownloadUsers = true,
                 };

                 config.Token = context.Configuration["app-token"];
             })
            .UseCommandService((context, config) =>
            {
                config.LogLevel = LogSeverity.Info;
                config.DefaultRunMode = RunMode.Async;
                config.CaseSensitiveCommands = false;
            })
            .UseInteractionService((context, config) =>
            {
                config.LogLevel = LogSeverity.Info;
                config.UseCompiledLambda = true;
            })
            .ConfigureLogging((host, config) =>
            {
                config.ClearProviders();
                config.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                config.AddNLog(host.Configuration);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<GlobalSettingService>();
                services.AddHostedService(provider => provider.GetRequiredService<GlobalSettingService>());

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
            })
            .Build();

        await host.RunAsync();
    }
}
