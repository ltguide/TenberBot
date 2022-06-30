using TenberBot.Data.Models;
using TenberBot.Data.Services;

namespace TenberBot.Services;

public class GlobalSettingService : BackgroundService
{
    private IReadOnlyList<GlobalSetting> Settings { get; set; } = new List<GlobalSetting>().AsReadOnly();

    private readonly IGlobalSettingDataService globalSettingDataService;
    private readonly ILogger<GlobalSettingService> logger;

    public GlobalSettingService(
        IGlobalSettingDataService globalSettingDataService,
        ILogger<GlobalSettingService> logger)
    {
        this.globalSettingDataService = globalSettingDataService;
        this.logger = logger;

        Console.WriteLine("wtf?");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Load();
    }

    public async Task Load()
    {
        try
        {
            Settings = await globalSettingDataService.GetAllRO().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GlobalSettingService startup");
        }
    }

    public T? Get<T>(string name)
    {
        var setting = Settings.FirstOrDefault(x => x.Name == name);
        if (setting == null)
            return default;

        return (T)(object)setting.Value;
    }
}
