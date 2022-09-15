using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TenberBot.Features.ExperienceFeature.Data.Enums;
using TenberBot.Features.ExperienceFeature.Settings.Channel;
using TenberBot.Shared.Features.Attributes.Modules;
using TenberBot.Shared.Features.Data.Models;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.Caches;
using TenberBot.Shared.Features.Extensions.Mentions;
using TenberBot.Shared.Features.Services;

namespace TenberBot.Features.ExperienceFeature.Modules.Interaction;

[Group("channel-experience", "Manage channel settings for the bot.")]
[HelpCommand(group: "Channel Management")]
[DefaultMemberPermissions(GuildPermission.ManageChannels)]
[EnabledInDm(false)]
public class ChannelExperienceInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IChannelSettingDataService channelSettingDataService;
    private readonly CacheService cacheService;

    public ChannelExperienceInteractionModule(
        IChannelSettingDataService channelSettingDataService,
        CacheService cacheService)
    {
        this.channelSettingDataService = channelSettingDataService;
        this.cacheService = cacheService;
    }

    [SlashCommand("modes", "Configure the experience mode.")]
    [HelpCommand("`[normal]` `[event]`")]
    public async Task SetExperience(
        [Summary("normal")] bool? normalEnabled = null,
        [Summary("event")] bool? eventEnabled = null,
        IChannel? channel = null)
    {
        channel ??= Context.Channel;

        if (channel is SocketThreadChannel thread)
            channel = thread.ParentChannel;

        await cacheService.Channel(channel);

        var settings = cacheService.Get<ExperienceChannelSettings>(channel);

        if (normalEnabled == true)
            settings.Mode |= ExperienceModes.Normal;
        else if (normalEnabled == false)
            settings.Mode &= ~ExperienceModes.Normal;

        if (eventEnabled == true)
            settings.Mode |= ExperienceModes.Event;
        else if (eventEnabled == false)
            settings.Mode &= ~ExperienceModes.Event;

        await Set(channel, settings);

        await RespondAsync($"Channel settings for *experience* in {channel.Id.GetChannelMention()}:{GetExperienceModeText(channel, ExperienceModes.Normal)}{GetExperienceModeText(channel, ExperienceModes.Event)}");
    }

    [SlashCommand("experience-values", "Configure the experience values.")]
    [HelpCommand("`<mode>` `[various ...]`")]
    public async Task SetExperienceValues(
        ExperienceModeChoice mode,
        decimal? message = null,
        [Summary("message-line")] decimal? messageLine = null,
        [Summary("message-word")] decimal? messageWord = null,
        [Summary("message-character")] decimal? messageCharacter = null,
        [Summary("message-attachment")] decimal? messageAttachment = null,
        [Summary("voice-minute")] decimal? voiceMinute = null,
        [Summary("voice-minute-video")] decimal? voiceMinuteVideo = null,
        [Summary("voice-minute-stream")] decimal? voiceMinuteStream = null,
        IChannel? channel = null)
    {
        channel ??= Context.Channel;

        if (channel is SocketThreadChannel thread)
            channel = thread.ParentChannel;

        await cacheService.Channel(channel);

        var settings = GetExperienceModeChannelSettings(channel, mode);

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
            await Set(channel, (NormalExperienceModeChannelSettings)settings);
        else
            await Set(channel, (EventExperienceModeChannelSettings)settings);

        await RespondAsync($"Channel settings for *experience* in {channel.Id.GetChannelMention()}:{GetExperienceModeText(channel, ExperienceModes.Normal)}{GetExperienceModeText(channel, ExperienceModes.Event)}");
    }

    private async Task Set<T>(IChannel channel, T value)
    {
        var key = CacheService.GetSettingsKey<T>();

        cacheService.Cache.Set(channel, key, value);

        var setting = await channelSettingDataService.GetByName(channel.Id, key);
        if (setting == null)
            await channelSettingDataService.Add(new ChannelSetting { GuildId = Context.Guild.Id, ChannelId = channel.Id, Name = key, }.SetValue(value));
        else
            await channelSettingDataService.Update(setting, new ChannelSetting().SetValue(value));
    }

    private string GetExperienceModeText(IChannel channel, ExperienceModes mode)
    {
        if (cacheService.Get<ExperienceChannelSettings>(channel).Mode.HasFlag(mode))
        {
            var settings = GetExperienceModeChannelSettings(channel, mode == ExperienceModes.Normal ? ExperienceModeChoice.Normal : ExperienceModeChoice.Event);

            var voice = "";
            if (channel.GetChannelType() == ChannelType.Voice)
                voice = $"\n\n> **voice-minute**: {settings.VoiceMinute}\n> **voice-minute-video**: {settings.VoiceMinuteVideo}\n> **voice-minute-stream**: {settings.VoiceMinuteStream}";

            return $"\n\n> **__{mode}__**: Enabled\n> **message**: {settings.Message}\n> **message-line**: {settings.MessageLine}\n> **message-word**: {settings.MessageWord}\n> **message-character**: {settings.MessageCharacter}\n> **message-attachment**: {settings.MessageAttachment}{voice}";
        }
        else
            return $"\n\n> **__{mode}__**: Disabled";
    }

    private IExperienceModeChannelSettings GetExperienceModeChannelSettings(IChannel channel, ExperienceModeChoice mode)
    {
        return mode switch
        {
            ExperienceModeChoice.Normal => cacheService.Get<NormalExperienceModeChannelSettings>(channel),
            ExperienceModeChoice.Event => cacheService.Get<EventExperienceModeChannelSettings>(channel),
            _ => throw new NotImplementedException(),
        };
    }

    public enum ExperienceModeChoice
    {
        Normal,
        Event,
    }
}
