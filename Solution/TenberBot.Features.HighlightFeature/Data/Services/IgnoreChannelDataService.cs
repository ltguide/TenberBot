using Microsoft.EntityFrameworkCore;
using TenberBot.Features.HighlightFeature.Data.Models;

namespace TenberBot.Features.HighlightFeature.Data.Services;

public interface IIgnoreChannelDataService
{
    Task<IList<IgnoreChannel>> GetAll();

    Task<IList<IgnoreChannel>> GetAll(ulong guildId, ulong userId);

    Task<IgnoreChannel?> Get(IgnoreChannel newObject);

    Task<IgnoreChannel?> GetByIndex(ulong guildId, ulong userId, int index);

    Task Add(IgnoreChannel newObject);

    Task Delete(IgnoreChannel dbObject);
}

public class IgnoreChannelDataService : IIgnoreChannelDataService
{
    private readonly DataContext dbContext;

    public IgnoreChannelDataService(DataContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IList<IgnoreChannel>> GetAll()
    {
        return await dbContext.IgnoreChannels
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IList<IgnoreChannel>> GetAll(ulong guildId, ulong userId)
    {
        return await dbContext.IgnoreChannels
            .Where(x => x.GuildId == guildId && x.UserId == userId)
            .OrderBy(x => x.IgnoreChannelId)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IgnoreChannel?> Get(IgnoreChannel newObject)
    {
        return await dbContext.IgnoreChannels
            .FirstOrDefaultAsync(x => x.GuildId == newObject.GuildId && x.UserId == newObject.UserId && x.IgnoreChannelId == newObject.IgnoreChannelId)
            .ConfigureAwait(false);
    }

    public async Task<IgnoreChannel?> GetByIndex(ulong guildId, ulong userId, int index)
    {
        if (index < 1)
            return null;

        return await dbContext.IgnoreChannels
            .Where(x => x.GuildId == guildId && x.UserId == userId)
            .OrderBy(x => x.IgnoreChannelId)
            .Skip(index - 1)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task Add(IgnoreChannel newObject)
    {
        if (newObject == null)
            throw new ArgumentNullException(nameof(newObject));

        newObject.HighlightIgnoreChannelId = 0;

        dbContext.Add(newObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Delete(IgnoreChannel dbObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        dbContext.Remove(dbObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
