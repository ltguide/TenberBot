using Microsoft.EntityFrameworkCore;
using TenberBot.Features.HighlightFeature.Data.Models;

namespace TenberBot.Features.HighlightFeature.Data.Services;

public interface IIgnoreUserDataService
{
    Task<IList<IgnoreUser>> GetAll();

    Task<IList<IgnoreUser>> GetAll(ulong guildId, ulong userId);

    Task<IgnoreUser?> Get(IgnoreUser newObject);

    Task<IgnoreUser?> GetByIndex(ulong guildId, ulong userId, int index);

    Task Add(IgnoreUser newObject);

    Task Delete(IgnoreUser dbObject);
}

public class IgnoreUserDataService : IIgnoreUserDataService
{
    private readonly DataContext dbContext;

    public IgnoreUserDataService(DataContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IList<IgnoreUser>> GetAll()
    {
        return await dbContext.IgnoreUsers
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IList<IgnoreUser>> GetAll(ulong guildId, ulong userId)
    {
        return await dbContext.IgnoreUsers
            .Where(x => x.GuildId == guildId && x.UserId == userId)
            .OrderBy(x => x.IgnoreUserId)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IgnoreUser?> Get(IgnoreUser newObject)
    {
        return await dbContext.IgnoreUsers
            .FirstOrDefaultAsync(x => x.GuildId == newObject.GuildId && x.UserId == newObject.UserId && x.IgnoreUserId == newObject.IgnoreUserId)
            .ConfigureAwait(false);
    }

    public async Task<IgnoreUser?> GetByIndex(ulong guildId, ulong userId, int index)
    {
        if (index < 1)
            return null;

        return await dbContext.IgnoreUsers
            .Where(x => x.GuildId == guildId && x.UserId == userId)
            .OrderBy(x => x.IgnoreUserId)
            .Skip(index - 1)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task Add(IgnoreUser newObject)
    {
        if (newObject == null)
            throw new ArgumentNullException(nameof(newObject));

        newObject.HighlightIgnoreUserId = 0;

        dbContext.Add(newObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Delete(IgnoreUser dbObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        dbContext.Remove(dbObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
