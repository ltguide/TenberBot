using TenberBot.Shared.Features.Data.Ids;

namespace TenberBot.Shared.Features.Data.POCO;

public class UserStatMod
{
    public GuildUserIds Ids { get; init; }

    public string UserStatType { get; init; }

    public int Value { get; set; } = 1;

    public bool Overwrite { get; set; }

    public UserStatMod(GuildUserIds ids, string userStatType)
    {
        Ids = ids;
        UserStatType = userStatType;
    }
}
