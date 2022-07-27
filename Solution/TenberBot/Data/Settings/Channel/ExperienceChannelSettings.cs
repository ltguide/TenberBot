using TenberBot.Attributes;
using TenberBot.Data.Enums;

namespace TenberBot.Data.Settings.Channel;

[ChannelSettings("experience")]
public class ExperienceChannelSettings
{
    public ExperienceMode Mode { get; set; } = ExperienceMode.Normal;

    public decimal VoiceMinute { get; set; } = 0.3m;

    public decimal VoiceMinuteVideo { get; set; } = 0.05m;

    public decimal VoiceMinuteStream { get; set; } = 0.11m;

    public decimal Message { get; set; } = 1m;

    public decimal MessageLine { get; set; } = 0.1m;

    public decimal MessageWord { get; set; } = 0.05m;

    public decimal MessageCharacter { get; set; } = 0.01m;

    public decimal MessageAttachment { get; set; } = 0.1m;
}
