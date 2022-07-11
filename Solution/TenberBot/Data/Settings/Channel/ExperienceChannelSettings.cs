using TenberBot.Attributes;

namespace TenberBot.Data.Settings.Channel;

[ChannelSettings("experience")]
public class ExperienceChannelSettings
{
    public bool Enabled { get; set; } = true;

    public decimal VoiceMinute { get; set; } = 1m;

    public decimal VoiceMinuteVideo { get; set; } = 0.1m;

    public decimal VoiceMinuteStream { get; set; } = 0.1m;

    public decimal Message { get; set; } = 1m;

    public decimal MessageLine { get; set; } = 0.1m;

    public decimal MessageWord { get; set; } = 0.05m;

    public decimal MessageCharacter { get; set; } = 0.01m;

    public decimal MessageAttachment { get; set; } = 0.1m;
}
