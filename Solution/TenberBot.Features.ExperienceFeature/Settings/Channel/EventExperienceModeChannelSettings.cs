using TenberBot.Shared.Features.Attributes.Settings;

namespace TenberBot.Features.ExperienceFeature.Settings.Channel;

[ChannelSettings("experience-event")]
public class EventExperienceModeChannelSettings : IExperienceModeChannelSettings
{
    public decimal VoiceMinute { get; set; } = 0;

    public decimal VoiceMinuteVideo { get; set; } = 0;

    public decimal VoiceMinuteStream { get; set; } = 0;

    public decimal Message { get; set; } = 0;

    public decimal MessageLine { get; set; } = 0;

    public decimal MessageWord { get; set; } = 0;

    public decimal MessageCharacter { get; set; } = 0;

    public decimal MessageAttachment { get; set; } = 0;
}
