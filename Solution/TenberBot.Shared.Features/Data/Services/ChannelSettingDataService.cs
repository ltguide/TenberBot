using Microsoft.EntityFrameworkCore;
using TenberBot.Shared.Features.Data.Models;

namespace TenberBot.Shared.Features.Data.Services;

public interface IChannelSettingDataService
{
    Task<IList<ChannelSetting>> GetAll(ulong channelId);

    Task<ChannelSetting?> GetByName(ulong channelId, string name);

    Task Add(ChannelSetting newObject);

    Task Update(ChannelSetting dbObject, ChannelSetting newObject);
}

public class ChannelSettingDataService : IChannelSettingDataService
{
    private readonly SharedDataContext dbContext;

    public ChannelSettingDataService(SharedDataContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IList<ChannelSetting>> GetAll(ulong channelId)
    {
        return await dbContext.ChannelSettings
            .Where(x => x.ChannelId == channelId)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<ChannelSetting?> GetByName(ulong channelId, string name)
    {
        return await dbContext.ChannelSettings
            .Where(x => x.ChannelId == channelId)
            .FirstOrDefaultAsync(x => x.Name == name)
            .ConfigureAwait(false);
    }

    public async Task Add(ChannelSetting newObject)
    {
        if (newObject == null)
            throw new ArgumentNullException(nameof(newObject));

        newObject.ChannelSettingId = 0;

        dbContext.Add(newObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Update(ChannelSetting dbObject, ChannelSetting newObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        if (newObject != null)
        {
            dbObject.Value = newObject.Value;
        }

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
