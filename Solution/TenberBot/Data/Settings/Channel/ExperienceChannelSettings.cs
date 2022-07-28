using TenberBot.Attributes;
using TenberBot.Data.Enums;

namespace TenberBot.Data.Settings.Channel;

[ChannelSettings("experience")]
public class ExperienceChannelSettings
{
    public ExperienceModes Mode { get; set; } = ExperienceModes.Normal;
}
