using TenberBot.Attributes;

namespace TenberBot.Data.Settings.Server;

[ServerSettings("basic")]
public class BasicServerSettings
{
    public string Prefix { get; set; } = "";
}
