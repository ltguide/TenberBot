using TenberBot.Shared.Features.Attributes.Settings;

namespace TenberBot.Features.ExperienceFeature.Settings.Server;

[ServerSettings("leaderboard")]
public class LeaderboardServerSettings
{
    public bool DisplayEventA { get; set; } = false;

    public bool DisplayEventB { get; set; } = false;
}
