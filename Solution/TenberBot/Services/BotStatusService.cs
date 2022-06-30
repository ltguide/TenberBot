using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;

namespace TenberBot.Services;

public class BotStatusService : DiscordClientService
{
    public BotStatusService(DiscordSocketClient client, ILogger<DiscordClientService> logger) : base(client, logger)
    {
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Client.WaitForReadyAsync(stoppingToken);
        Logger.LogInformation("Client is ready!");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Client.SetGameAsync($"whatevs yo {new Random().Next()}");

            await Task.Delay(2 * 60 * 1000, stoppingToken);
        }
    }
}