using TenberBot.Data.Enums;

namespace TenberBot.Data.POCO;

public class LeaderboardView
{
    public int PageNumber { get; set; }

    public int PageCount { get; set; }

    public LeaderboardType LeaderboardType { get; set; }
}
