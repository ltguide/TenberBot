using Discord;
using Microsoft.Extensions.Caching.Memory;
using TenberBot.Extensions;

namespace TenberBot.Data.POCO;

public class ChannelExperience
{
    public bool Enabled { get; set; }

    public decimal Message { get; set; }

    public decimal MessageLine { get; set; }

    public decimal MessageWord { get; set; }

    public decimal MessageCharacter { get; set; }

    public decimal MessageAttachment { get; set; }

    public decimal VoiceMinute { get; set; }

    public ChannelExperience(IChannel channel, IMemoryCache cache)
    {
        Enabled = cache.Get<bool>(channel, ChannelSettings.ExperienceEnabled);
        Message = cache.Get<decimal>(channel, ChannelSettings.ExperienceMessage);
        MessageLine = cache.Get<decimal>(channel, ChannelSettings.ExperienceMessageLine);
        MessageWord = cache.Get<decimal>(channel, ChannelSettings.ExperienceMessageWord);
        MessageCharacter = cache.Get<decimal>(channel, ChannelSettings.ExperienceMessageCharacter);
        MessageAttachment = cache.Get<decimal>(channel, ChannelSettings.ExperienceMessageAttachment);
        VoiceMinute = cache.Get<decimal>(channel, ChannelSettings.ExperienceVoiceMinute);
    }
}
