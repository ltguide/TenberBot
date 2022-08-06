using Microsoft.EntityFrameworkCore;
using TenberBot.Shared.Features.Data.Models;

namespace TenberBot.Shared.Features.Data.Services;

public interface IServerSettingDataService
{
    Task<IList<ServerSetting>> GetAll(ulong guildId);

    Task<ServerSetting?> GetByName(ulong guildId, string name);

    Task Add(ServerSetting newObject);

    Task Update(ServerSetting dbObject, ServerSetting newObject);
}

public class ServerSettingDataService : IServerSettingDataService
{
    private readonly SharedDataContext dbContext;

    public ServerSettingDataService(SharedDataContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IList<ServerSetting>> GetAll(ulong guildId)
    {
        return await dbContext.ServerSettings
            .Where(x => x.GuildId == guildId)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<ServerSetting?> GetByName(ulong guildId, string name)
    {
        return await dbContext.ServerSettings
            .Where(x => x.GuildId == guildId)
            .FirstOrDefaultAsync(x => x.Name == name)
            .ConfigureAwait(false);
    }

    public async Task Add(ServerSetting newObject)
    {
        if (newObject == null)
            throw new ArgumentNullException(nameof(newObject));

        newObject.ServerSettingId = 0;

        dbContext.Add(newObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Update(ServerSetting dbObject, ServerSetting newObject)
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
