using Microsoft.EntityFrameworkCore;
using TenberBot.Data.Models;

namespace TenberBot.Data.Services;

public interface IRankCardDataService
{
    Task<IList<RankCard>> GetAllByGuildId(ulong guildId);

    Task<RankCard?> GetByRoleId(ulong roleId);

    Task Add(RankCard newObject);

    Task Update(RankCard dbObject, RankCard newObject);

    Task Delete(RankCard dbObject);
}

public class RankCardDataService : IRankCardDataService
{
    private readonly DataContext dbContext;

    public RankCardDataService(DataContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IList<RankCard>> GetAllByGuildId(ulong guildId)
    {
        return await dbContext.RankCards
            .Where(x => x.GuildId == guildId)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<RankCard?> GetByRoleId(ulong roleId)
    {
        return await dbContext.RankCards
            .FirstOrDefaultAsync(x => x.RoleId == roleId)
            .ConfigureAwait(false);
    }

    public async Task Add(RankCard newObject)
    {
        if (newObject == null)
            throw new ArgumentNullException(nameof(newObject));

        newObject.RankCardId = 0;

        dbContext.Add(newObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Update(RankCard dbObject, RankCard newObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        if (newObject != null)
        {
        }

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Delete(RankCard dbObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        dbContext.Remove(dbObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
