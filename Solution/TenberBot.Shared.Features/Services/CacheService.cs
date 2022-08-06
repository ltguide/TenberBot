using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.Caches;

namespace TenberBot.Shared.Features.Services;

public class CacheService : DiscordClientService
{
    public IMemoryCache Cache { get; }

    private readonly IChannelSettingDataService channelSettingDataService;
    private readonly IServerSettingDataService serverSettingDataService;

    public CacheService(
        IChannelSettingDataService channelSettingDataService,
        IServerSettingDataService serverSettingDataService,
        IMemoryCache memoryCache,
        DiscordSocketClient client,
        ILogger<CacheService> logger) : base(client, logger)
    {
        this.channelSettingDataService = channelSettingDataService;
        this.serverSettingDataService = serverSettingDataService;
        Cache = memoryCache;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Client.GuildAvailable += GuildAvailable;

        return Task.CompletedTask;
    }

    private async Task GuildAvailable(SocketGuild guild)
    {
        Logger.LogInformation($"Guild Available: {guild.Name} ({guild.Id})");

        await Guild(guild);
    }

    public async Task Guild(IGuild guild)
    {
        if (Cache.Get<bool>(guild, "cached"))
            return;

        var settings = await serverSettingDataService.GetAll(guild.Id);

        foreach (var setting in SharedFeatures.ServerSettings)
            Cache.Set(guild, setting.Value, settings.FirstOrDefault(x => x.Name == setting.Value)?.GetValue(setting.Key) ?? Activator.CreateInstance(setting.Key));

        Cache.Set(guild, "cached", true);
    }

    public async Task Channel(IChannel channel)
    {
        if (Cache.Get<bool>(channel, "cached"))
            return;

        var settings = await channelSettingDataService.GetAll(channel.Id);

        foreach (var setting in SharedFeatures.ChannelSettings)
            Cache.Set(channel, setting.Value, settings.FirstOrDefault(x => x.Name == setting.Value)?.GetValue(setting.Key) ?? Activator.CreateInstance(setting.Key));

        Cache.Set(channel, "cached", true);
    }

    public T Get<T>(IEntity<ulong> entity)
    {
        return Cache.Get<T>(entity, GetSettingsKey<T>());
    }

    public bool TryGetValue<T>(IEntity<ulong> entity, out T value)
    {
        return Cache.TryGetValue(entity, GetSettingsKey<T>(), out value);
    }

    public T Set<T>(IEntity<ulong> entity, T value)
    {
        return Cache.Set(entity, GetSettingsKey<T>(), value);
    }

    public static string GetSettingsKey<T>()
    {
        if (SharedFeatures.ServerSettings.TryGetValue(typeof(T), out var key))
            return key;

        if (SharedFeatures.ChannelSettings.TryGetValue(typeof(T), out key))
            return key;

        throw new InvalidOperationException("no key found");
    }
}
