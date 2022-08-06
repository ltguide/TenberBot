using TenberBot.Features.ExperienceFeature.Data.Enums;
using TenberBot.Shared.Features.Attributes.Settings;

namespace TenberBot.Features.ExperienceFeature.Settings.Channel;

[ChannelSettings("experience")]
public class ExperienceChannelSettings
{
    public ExperienceModes Mode { get; set; } = ExperienceModes.Normal;
}
