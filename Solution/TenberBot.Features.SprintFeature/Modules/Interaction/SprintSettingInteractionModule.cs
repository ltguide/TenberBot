using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TenberBot.Features.SprintFeature.Data.Enums;
using TenberBot.Features.SprintFeature.Settings.Channel;
using TenberBot.Shared.Features.Attributes.Modules;
using TenberBot.Shared.Features.Data.Models;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.Caches;
using TenberBot.Shared.Features.Services;

namespace TenberBot.Features.SprintFeature.Modules.Interaction;

[DefaultMemberPermissions(GuildPermission.ManageChannels)]
[HelpCommand(group: "Channel Management")]
[EnabledInDm(false)]
public class ChannelSprintInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IChannelSettingDataService channelSettingDataService;
    private readonly CacheService cacheService;

    public ChannelSprintInteractionModule(
        IChannelSettingDataService channelSettingDataService,
        CacheService cacheService)
    {
        this.channelSettingDataService = channelSettingDataService;
        this.cacheService = cacheService;
    }

    [SlashCommand("channel-sprint", "Configure the sprint mode/role.")]
    [HelpCommand("`[mode]` `[role]`")]
    public async Task SetSprint(SprintMode? mode = null, IRole? role = null)
    {
        if (Context.Channel is SocketThreadChannel)
        {
            await RespondAsync("Threads use the settings of their parent channel.", ephemeral: true);
            return;
        }

        var settings = cacheService.Get<SprintChannelSettings>(Context.Channel);

        if (mode != null)
            settings.Mode = mode.Value;

        if (role != null)
            settings.Role = role.Mention;

        await Set(settings);

        await RespondAsync($"Channel settings for *sprint*:\n\n> **Mode**: {settings.Mode}\n> **Role**: {settings.Role}");
    }

    private async Task Set<T>(T value)
    {
        var key = CacheService.GetSettingsKey<T>();

        cacheService.Cache.Set(Context.Channel, key, value);

        var setting = await channelSettingDataService.GetByName(Context.Channel.Id, key);
        if (setting == null)
            await channelSettingDataService.Add(new ChannelSetting { GuildId = Context.Guild.Id, ChannelId = Context.Channel.Id, Name = key, }.SetValue(value));
        else
            await channelSettingDataService.Update(setting, new ChannelSetting().SetValue(value));
    }
}
