using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Data.Services;
using TenberBot.Data.Settings.Channel;
using TenberBot.Extensions;
using TenberBot.Services;

namespace TenberBot.Modules.Interaction;

[Group("channel-setting", "Manage channel settings for the bot.")]
[DefaultMemberPermissions(GuildPermission.ManageChannels)]
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

    [SlashCommand("sprint", "Configure the sprint mode/role.")]
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

    [SlashCommand("experience", "Configure the experience mode.")]
    public async Task SetExperience(
        [Summary("normal")] bool? normalEnabled = null,
        [Summary("event")] bool? eventEnabled = null)
    {
        if (Context.Channel is SocketThreadChannel)
        {
            await RespondAsync("Threads use the settings of their parent channel.", ephemeral: true);
            return;
        }

        var settings = cacheService.Get<ExperienceChannelSettings>(Context.Channel);

        if (normalEnabled == true)
            settings.Mode |= ExperienceModes.Normal;
        else if (normalEnabled == false)
            settings.Mode &= ~ExperienceModes.Normal;

        if (eventEnabled == true)
            settings.Mode |= ExperienceModes.Event;
        else if (eventEnabled == false)
            settings.Mode &= ~ExperienceModes.Event;

        await Set(settings);

        await RespondAsync($"Channel settings for *experience*:{GetExperienceModeText(ExperienceModes.Normal)}{GetExperienceModeText(ExperienceModes.Event)}");
    }

    [SlashCommand("experience-values", "Configure the experience values.")]
    public async Task SetExperienceValues(
    ExperienceModeChoice mode,
    decimal? message = null,
    [Summary("message-line")] decimal? messageLine = null,
    [Summary("message-word")] decimal? messageWord = null,
    [Summary("message-character")] decimal? messageCharacter = null,
    [Summary("message-attachment")] decimal? messageAttachment = null,
    [Summary("voice-minute")] decimal? voiceMinute = null,
    [Summary("voice-minute-video")] decimal? voiceMinuteVideo = null,
    [Summary("voice-minute-stream")] decimal? voiceMinuteStream = null)
    {
        if (Context.Channel is SocketThreadChannel)
        {
            await RespondAsync("Threads use the settings of their parent channel.", ephemeral: true);
            return;
        }

        var settings = GetExperienceModeChannelSettings(mode);

        if (message != null)
            settings.Message = message.Value;

        if (messageLine != null)
            settings.MessageLine = messageLine.Value;

        if (messageWord != null)
            settings.MessageWord = messageWord.Value;

        if (messageCharacter != null)
            settings.MessageCharacter = messageCharacter.Value;

        if (messageAttachment != null)
            settings.MessageAttachment = messageAttachment.Value;

        if (voiceMinute != null)
            settings.VoiceMinute = voiceMinute.Value;

        if (voiceMinuteVideo != null)
            settings.VoiceMinuteVideo = voiceMinuteVideo.Value;

        if (voiceMinuteStream != null)
            settings.VoiceMinuteStream = voiceMinuteStream.Value;

        if (mode == ExperienceModeChoice.Normal)
            await Set((NormalExperienceModeChannelSettings)settings);
        else
            await Set((EventExperienceModeChannelSettings)settings);

        await RespondAsync($"Channel settings for *experience*:{GetExperienceModeText(ExperienceModes.Normal)}{GetExperienceModeText(ExperienceModes.Event)}");
    }

    private async Task Set<T>(T value)
    {
        var key = cacheService.GetSettingsKey<T>();

        cacheService.Cache.Set(Context.Channel, key, value);

        var setting = await channelSettingDataService.GetByName(Context.Channel.Id, key);
        if (setting == null)
            await channelSettingDataService.Add(new ChannelSetting { GuildId = Context.Guild.Id, ChannelId = Context.Channel.Id, Name = key, }.SetValue(value));
        else
            await channelSettingDataService.Update(setting, new ChannelSetting().SetValue(value));
    }

    private string GetExperienceModeText(ExperienceModes mode)
    {
        if (cacheService.Get<ExperienceChannelSettings>(Context.Channel).Mode.HasFlag(mode))
        {
            var settings = GetExperienceModeChannelSettings(mode == ExperienceModes.Normal ? ExperienceModeChoice.Normal : ExperienceModeChoice.Event);

            var voice = "";
            if (Context.Channel.GetChannelType() == ChannelType.Voice)
                voice = $"\n\n> **voice-minute**: {settings.VoiceMinute}\n> **voice-minute-video**: {settings.VoiceMinuteVideo}\n> **voice-minute-stream**: {settings.VoiceMinuteStream}";

            return $"\n\n> **__{mode}__**: Enabled\n> **message**: {settings.Message}\n> **message-line**: {settings.MessageLine}\n> **message-word**: {settings.MessageWord}\n> **message-character**: {settings.MessageCharacter}\n> **message-attachment**: {settings.MessageAttachment}{voice}";
        }
        else
            return $"\n\n> **__{mode}__**: Disabled";
    }

    private IExperienceModeChannelSettings GetExperienceModeChannelSettings(ExperienceModeChoice mode)
    {
        return mode switch
        {
            ExperienceModeChoice.Normal => cacheService.Get<NormalExperienceModeChannelSettings>(Context.Channel),
            ExperienceModeChoice.Event => cacheService.Get<EventExperienceModeChannelSettings>(Context.Channel),
            _ => throw new NotImplementedException(),
        };
    }

    public enum ExperienceModeChoice
    {
        Normal,
        Event,
    }
}
