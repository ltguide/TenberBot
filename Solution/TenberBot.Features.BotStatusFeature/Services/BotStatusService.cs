using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using TenberBot.Features.BotStatusFeature.Data.Services;

namespace TenberBot.Features.BotStatusFeature.Services;

public class BotStatusService : DiscordClientService
{
    private readonly IBotStatusDataService botStatusDataService;

    public BotStatusService(
        IBotStatusDataService botStatusDataService,
        DiscordSocketClient client,
        ILogger<BotStatusService> logger) : base(client, logger)
    {
        this.botStatusDataService = botStatusDataService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Client.WaitForReadyAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var botStatus = await botStatusDataService.GetRandom();

            if (botStatus != null)
                await Client.SetGameAsync(botStatus.Text);
            else
                await Client.SetGameAsync("");

            await Task.Delay(TimeSpan.FromMinutes(4), stoppingToken);
        }
    }
}
