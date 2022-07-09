using Discord;
using Discord.Interactions;
using TenberBot.Data;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Data.Services;
using TenberBot.Extensions;
using TenberBot.Services;

namespace TenberBot.Modules.Interaction;

[Group("channel-setting", "Manage channel settings for the bot.")]
[RequireUserPermission(ChannelPermission.ManageChannels)]
public class ChannelSettingInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IChannelSettingDataService channelSettingDataService;
    private readonly CacheService cacheService;

    public ChannelSettingInteractionModule(
        IChannelSettingDataService channelSettingDataService,
        CacheService cacheService)
    {
        this.channelSettingDataService = channelSettingDataService;
        this.cacheService = cacheService;
    }

    [SlashCommand("sprint", "Set the prefix for message commands.")]
    public async Task SetSprint(SprintMode mode, IRole role)
    {
        await Set(ChannelSettings.SprintMode, mode.ToString(), mode);
        await Set(ChannelSettings.SprintRole, role.Mention, role.Mention);

        await RespondAsync($"Channel settings for *sprint* updated.\n\n> **Mode**: {mode}\n> **Role**: {role.Name}");
    }

    private async Task Set<T>(string name, string value, T cacheValue)
    {
        cacheService.Cache.Set(Context.Guild, name, cacheValue);

        var setting = await channelSettingDataService.GetByName(Context.Guild.Id, name);
        if (setting == null)
            await channelSettingDataService.Add(new ChannelSetting { GuildId = Context.Guild.Id, ChannelId = Context.Channel.Id, Name = name, Value = value, });
        else
            await channelSettingDataService.Update(setting, new ChannelSetting { Value = value, });
    }
}
