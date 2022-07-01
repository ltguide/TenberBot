using Discord;
using TenberBot.Data;
using TenberBot.Data.Services;

namespace TenberBot.Services;

public class GlobalSettingService : BackgroundService
{
    private readonly IGlobalSettingDataService globalSettingDataService;
    private readonly ILogger<GlobalSettingService> logger;

    public GlobalSettingService(
        IGlobalSettingDataService globalSettingDataService,
        ILogger<GlobalSettingService> logger)
    {
        this.globalSettingDataService = globalSettingDataService;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var settings = (await globalSettingDataService.GetAll().ConfigureAwait(false)).ToDictionary(x => x.Name, x => x.Value);

            if (settings.TryGetValue("prefix", out var value))
                GlobalSettings.Prefix = value;

            if (settings.TryGetValue("emote-success", out value) && Emote.TryParse(value, out var emote))
                GlobalSettings.EmoteSuccess = emote;

            if (settings.TryGetValue("emote-fail", out value) && Emote.TryParse(value, out emote))
                GlobalSettings.EmoteFail = emote;

            if (settings.TryGetValue("emote-unknown", out value) && Emote.TryParse(value, out emote))
                GlobalSettings.EmoteUnknown = emote;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GlobalSettingService startup");
        }
    }
}
