using TenberBot.Attributes;

namespace TenberBot.Data.Settings.Server;

[ServerSettings("rank")]
public class RankServerSettings
{
    public IList<RankCardSettings> Cards { get; set; } = new List<RankCardSettings>();
}
