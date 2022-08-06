using TenberBot.Shared.Features.Attributes.Settings;

namespace TenberBot.Shared.Features.Settings.Server;

[ServerSettings("basic")]
public class BasicServerSettings
{
    public string Prefix { get; set; } = "";
}
