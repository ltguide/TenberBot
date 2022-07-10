using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TenberBot.Data;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Data.POCO;
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
    public async Task SetSprint(SprintMode? mode = null, IRole? role = null)
    {
        if (Context.Channel is SocketThreadChannel)
        {
            await RespondAsync("Threads use the settings of their parent channel.", ephemeral: true);
            return;
        }

        if (mode != null)
            await Set(ChannelSettings.SprintMode, mode.Value.ToString(), mode);

        if (role != null)
            await Set(ChannelSettings.SprintRole, role.Mention, role.Mention);

        mode = cacheService.Cache.Get<SprintMode>(Context.Channel, ChannelSettings.SprintMode);
        var sprintRole = cacheService.Cache.Get<string>(Context.Channel, ChannelSettings.SprintRole);

        await RespondAsync($"Channel settings for *sprint*:\n\n> **Mode**: {mode}\n> **Role**: {sprintRole}");
    }

    [SlashCommand("experience", "Set the experience for leveling.")]
    public async Task SetExperience(
        bool? enabled = null,
        [Summary("message")] decimal? message = null,
        [Summary("message-line")] decimal? messageLine = null,
        [Summary("message-word")] decimal? messageWord = null,
        [Summary("message-character")] decimal? messageCharacter = null,
        [Summary("message-attachment")] decimal? messageAttachment = null,
        [Summary("voice-minute")] decimal? voiceMinute = null)
    {
        if (Context.Channel is SocketThreadChannel)
        {
            await RespondAsync("Threads use the settings of their parent channel.", ephemeral: true);
            return;
        }

        if (enabled != null)
            await Set(ChannelSettings.ExperienceEnabled, enabled.Value.ToString(), enabled);

        if (message != null)
            await Set(ChannelSettings.ExperienceMessage, message.Value.ToString(), message);

        if (messageLine != null)
            await Set(ChannelSettings.ExperienceMessageLine, messageLine.Value.ToString(), messageLine);

        if (messageWord != null)
            await Set(ChannelSettings.ExperienceMessageWord, messageWord.Value.ToString(), messageWord);

        if (messageCharacter != null)
            await Set(ChannelSettings.ExperienceMessageCharacter, messageCharacter.Value.ToString(), messageCharacter);

        if (messageAttachment != null)
            await Set(ChannelSettings.ExperienceMessageAttachment, messageAttachment.Value.ToString(), messageAttachment);

        if (voiceMinute != null)
            await Set(ChannelSettings.ExperienceVoiceMinute, voiceMinute.Value.ToString(), voiceMinute);

        var channelExperience = new ChannelExperience(Context.Channel, cacheService.Cache);

        if (channelExperience.Enabled)
        {
            var voice = "";
            if (Context.Channel.GetChannelType() == ChannelType.Voice)
                voice = $"\n> **voice-minute**: {channelExperience.VoiceMinute}";

            await RespondAsync($"Channel settings for *experience*:\n\n> **status**: Enabled\n\n> **message**: {channelExperience.Message}\n> **message-line**: {channelExperience.MessageLine}\n> **message-word**: {channelExperience.MessageWord}\n> **message-character**: {channelExperience.MessageCharacter}\n> **message-attachment**: {channelExperience.MessageAttachment}{voice}");
        }
        else
            await RespondAsync("Channel settings for *experience*:\n\n> **status**: Disabled");
    }

    private async Task Set<T>(string name, string value, T cacheValue)
    {
        cacheService.Cache.Set(Context.Channel, name, cacheValue);

        var setting = await channelSettingDataService.GetByName(Context.Channel.Id, name);
        if (setting == null)
            await channelSettingDataService.Add(new ChannelSetting { GuildId = Context.Guild.Id, ChannelId = Context.Channel.Id, Name = name, Value = value, });
        else
            await channelSettingDataService.Update(setting, new ChannelSetting { Value = value, });
    }
}
