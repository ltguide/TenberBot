using Microsoft.EntityFrameworkCore;
using TenberBot.Shared.Features.Data.Models;
using TenberBot.Shared.Features.Data.POCO;

namespace TenberBot.Shared.Features.Data.Services;

public interface IUserStatDataService
{
    Task<IDictionary<string, UserStat>> Get(UserStatMod[] userStatMods);

    Task<UserStat> Get(UserStatMod userStatMod);

    Task<IDictionary<string, UserStat>> Update(UserStatMod[] userStatMods);

    Task<UserStat> Update(UserStatMod userStatMod);

    Task Save();
}

public class UserStatDataService : IUserStatDataService
{
    private readonly SharedDataContext dbContext;

    public UserStatDataService(SharedDataContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IDictionary<string, UserStat>> Get(UserStatMod[] userStatMods)
    {
        var results = new Dictionary<string, UserStat>();

        foreach (var userStatMod in userStatMods)
            results.Add(userStatMod.UserStatType, await Get(userStatMod).ConfigureAwait(false));

        return results;
    }

    public async Task<UserStat> Get(UserStatMod userStatMod)
    {
        var result = await dbContext.UserStats
            .FirstOrDefaultAsync(x => x.GuildId == userStatMod.Ids.GuildId && x.UserId == userStatMod.Ids.UserId && x.UserStatType == userStatMod.UserStatType)
            .ConfigureAwait(false);

        if (result == null)
        {
            result = new UserStat
            {
                UserStatId = 0,
                GuildId = userStatMod.Ids.GuildId,
                UserId = userStatMod.Ids.UserId,
                UserStatType = userStatMod.UserStatType,
                Value = 0,
            };

            dbContext.Add(result);
        }

        return result;
    }

    public async Task<IDictionary<string, UserStat>> Update(UserStatMod[] userStatMods)
    {
        var results = new Dictionary<string, UserStat>();

        foreach (var userStatMod in userStatMods)
            results.Add(userStatMod.UserStatType, await Add(userStatMod).ConfigureAwait(false));

        await Save().ConfigureAwait(false);

        return results;
    }

    public async Task<UserStat> Update(UserStatMod userStatMod)
    {
        var result = await Add(userStatMod).ConfigureAwait(false);

        await Save().ConfigureAwait(false);

        return result;
    }

    public async Task<UserStat> Add(UserStatMod userStatMod)
    {
        var result = await Get(userStatMod).ConfigureAwait(false);

        if (userStatMod.Overwrite)
            result.Value = userStatMod.Value;
        else
            result.Value += userStatMod.Value;

        return result;
    }

    public Task Save()
    {
        return dbContext.SaveChangesAsync();
    }
}
