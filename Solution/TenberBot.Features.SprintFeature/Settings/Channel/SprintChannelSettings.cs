using TenberBot.Features.SprintFeature.Data.Enums;
using TenberBot.Shared.Features.Attributes.Settings;

namespace TenberBot.Features.SprintFeature.Settings.Channel;

[ChannelSettings("sprint")]
public class SprintChannelSettings
{
    public SprintMode Mode { get; set; } = SprintMode.Disabled;

    public string Role { get; set; } = "everyone";
}
