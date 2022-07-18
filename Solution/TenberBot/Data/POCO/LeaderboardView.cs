using TenberBot.Data.Enums;

namespace TenberBot.Data.POCO;

public class LeaderboardView
{
    public int MinimumExperience { get; set; } = 50;

    public int PerPage { get; set; } = 10;

    public int CurrentPage { get; set; }

    public int UserPage { get; set; }

    public int PageCount { get; set; }

    public LeaderboardType LeaderboardType { get; set; }

    public int BaseRank => (PerPage * CurrentPage) + 1;

    public int GetNewPage(string page)
    {
        return page switch
        {
            "first" => 0,
            "previous" => Math.Max(0, CurrentPage - 1),
            "user" => Math.Max(0, UserPage),
            "next" => Math.Min(PageCount, CurrentPage + 1),
            "last" => PageCount,
            "refresh" => CurrentPage,
            _ => throw new NotImplementedException(),
        };
    }
}
