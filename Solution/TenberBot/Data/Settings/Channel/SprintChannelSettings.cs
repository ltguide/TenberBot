using TenberBot.Attributes;
using TenberBot.Data.Enums;

namespace TenberBot.Data.Settings.Channel;

[ChannelSettings("sprint")]
public class SprintChannelSettings
{
    public SprintMode Mode { get; set; } = SprintMode.Disabled;

    public string Role { get; set; } = "everyone";
}
