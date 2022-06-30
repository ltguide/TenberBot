using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using TenberBot.Data.Services;

namespace TenberBot.Services;

public class BotStatusService : DiscordClientService
{
    private readonly IBotStatusDataService botStatusDataService;

    public BotStatusService(
        IBotStatusDataService botStatusDataService,
        DiscordSocketClient client,
        ILogger<DiscordClientService> logger) : base(client, logger)
    {
        this.botStatusDataService = botStatusDataService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Client.WaitForReadyAsync(stoppingToken);
        Logger.LogInformation("Client is ready!");

        while (!stoppingToken.IsCancellationRequested)
        {
            var botStatus = await botStatusDataService.GetRandom();

            if (botStatus != null)
                await Client.SetGameAsync(botStatus.Text);
            else
                await Client.SetGameAsync("");

            await Task.Delay(2 * 60 * 1000, stoppingToken);
        }
    }
}