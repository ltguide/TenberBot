using TenberBot.Attributes;

namespace TenberBot.Data.Settings.Server;

[ServerSettings("leaderboard")]
public class LeaderboardServerSettings
{
    public bool DisplayEvent { get; set; } = false;
}
