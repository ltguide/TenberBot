using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using TenberBot.Data;
using TenberBot.Data.Services;
using TenberBot.Extensions;

namespace TenberBot.Services;

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

        var settings = (await serverSettingDataService.GetAll(guild.Id)).ToDictionary(x => x.Name, x => x.Value);

        Map(guild, settings, ServerSettings.Defaults);

        Cache.Set(guild, "cached", true);
    }

    public async Task Channel(IChannel channel)
    {
        if (Cache.Get<bool>(channel, "cached"))
            return;

        var settings = (await channelSettingDataService.GetAll(channel.Id)).ToDictionary(x => x.Name, x => x.Value);

        Map(channel, settings, ChannelSettings.Defaults);

        Cache.Set(channel, "cached", true);
    }

    private void Map(IEntity<ulong> entity, IDictionary<string, string> settings, IReadOnlyDictionary<string, object> defaults)
    {
        foreach (var kvp in defaults)
        {
            kvp.Deconstruct(out var key, out var defaultValue);

            if (settings.TryGetValue(key, out var value))
            {
                if (defaultValue is IEmote)
                    Cache.Set(entity, key, value.AsIEmote() ?? defaultValue);

                else if (defaultValue is Enum)
                    Cache.Set(entity, key, Enum.TryParse(defaultValue.GetType(), value, out var @enum) ? @enum : defaultValue);

                else if (defaultValue is decimal)
                    Cache.Set(entity, key, decimal.TryParse(value, out var @decimal) ? @decimal : defaultValue);

                else if (defaultValue is ulong)
                    Cache.Set(entity, key, ulong.TryParse(value, out var @ulong) ? @ulong : defaultValue);

                else if (defaultValue is bool)
                    Cache.Set(entity, key, bool.TryParse(value, out var @bool) ? @bool : defaultValue);

                else
                    Cache.Set(entity, key, value);
            }
            else
                Cache.Set(entity, key, defaultValue);
        }
    }
}
