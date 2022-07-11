using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;
using TenberBot.Attributes;
using TenberBot.Data.Services;
using TenberBot.Extensions;

namespace TenberBot.Services;

public class CacheService : DiscordClientService
{
    public IMemoryCache Cache { get; }

    private Dictionary<Type, string> ServerSettings = new();
    private Dictionary<Type, string> ChannelSettings = new();
    private Dictionary<Type, string> AllSettings = new();

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

        ServerSettings = GetTypesByAttribute<ServerSettingsAttribute>(Assembly.GetEntryAssembly());

        ChannelSettings = GetTypesByAttribute<ChannelSettingsAttribute>(Assembly.GetEntryAssembly());

        AllSettings = ServerSettings.Concat(ChannelSettings).ToDictionary(x => x.Key, x => x.Value);

        return Task.CompletedTask;
    }

    private static Dictionary<Type, string> GetTypesByAttribute<T>(Assembly? assembly) where T : SettingsAttribute
    {
        if (assembly == null)
            throw new ArgumentNullException(nameof(assembly));

        return assembly.GetTypes().Where(x => x.GetCustomAttribute<T>() != null).ToDictionary(x => x, x => x.GetCustomAttribute<T>()!.Key);
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

        foreach (var setting in ServerSettings)
            Cache.Set(guild, setting.Value, settings.FirstOrDefault(x => x.Name == setting.Value)?.GetValue(setting.Key) ?? Activator.CreateInstance(setting.Key));

        Cache.Set(guild, "cached", true);
    }

    public async Task Channel(IChannel channel)
    {
        if (Cache.Get<bool>(channel, "cached"))
            return;

        var settings = await channelSettingDataService.GetAll(channel.Id);

        foreach (var setting in ChannelSettings)
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

    public string GetSettingsKey<T>()
    {
        if (AllSettings.TryGetValue(typeof(T), out var key) == false)
            throw new InvalidOperationException("no key found");

        return key;
    }
}
