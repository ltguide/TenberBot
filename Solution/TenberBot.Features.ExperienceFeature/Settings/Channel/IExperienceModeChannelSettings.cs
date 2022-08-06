namespace TenberBot.Features.ExperienceFeature.Settings.Channel;

public interface IExperienceModeChannelSettings
{
    public decimal VoiceMinute { get; set; }

    public decimal VoiceMinuteVideo { get; set; }

    public decimal VoiceMinuteStream { get; set; }

    public decimal Message { get; set; }

    public decimal MessageLine { get; set; }

    public decimal MessageWord { get; set; }

    public decimal MessageCharacter { get; set; }

    public decimal MessageAttachment { get; set; }
}
