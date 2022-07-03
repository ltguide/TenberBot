using Microsoft.EntityFrameworkCore;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;

namespace TenberBot.Data.Services;

public interface ISprintDataService
{
    Task<IList<Sprint>> GetAllActive();

    Task<Sprint?> GetById(int id);

    Task Add(Sprint newObject);

    Task Update(Sprint dbObject, Sprint newObject);


    Task<UserSprint?> GetUserById(ulong userId, bool active);


    Task<SprintChannel?> GetChannelById(ulong channelId);

    Task Add(SprintChannel newObject);

    Task Update(SprintChannel dbObject, SprintChannel newObject);
}

public class SprintDataService : ISprintDataService
{
    private readonly DataContext dbContext;

    public SprintDataService(DataContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IList<Sprint>> GetAllActive()
    {
        return await dbContext.Sprints
            .Include(x => x.Users)
            .Where(x => x.SprintStatus == SprintStatus.Waiting || x.SprintStatus == SprintStatus.Started)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<Sprint?> GetById(int id)
    {
        return await dbContext.Sprints
            .Include(x => x.Users)
            .FirstOrDefaultAsync(x => x.SprintId == id)
            .ConfigureAwait(false);
    }

    public async Task Add(Sprint newObject)
    {
        if (newObject == null)
            throw new ArgumentNullException(nameof(newObject));

        newObject.SprintId = 0;

        dbContext.Add(newObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Update(Sprint dbObject, Sprint newObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        if (newObject != null)
        {
            dbObject.SprintStatus = newObject.SprintStatus;
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task<UserSprint?> GetUserById(ulong userId, bool active)
    {
        return await dbContext.UserSprints
            .Include(x => x.Sprint)
            .ThenInclude(x => x.Users)
            .Where(x => x.Sprint.SprintStatus == SprintStatus.Waiting || x.Sprint.SprintStatus == SprintStatus.Started)
            .FirstOrDefaultAsync(x => x.UserId == userId)
            .ConfigureAwait(false);
    }

    public async Task<SprintChannel?> GetChannelById(ulong channelId)
    {
        return await dbContext.SprintChannels
            .FirstOrDefaultAsync(x => x.ChannelId == channelId)
            .ConfigureAwait(false);
    }

    public async Task Add(SprintChannel newObject)
    {
        if (newObject == null)
            throw new ArgumentNullException(nameof(newObject));

        newObject.SprintChannelId = 0;

        dbContext.Add(newObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Update(SprintChannel dbObject, SprintChannel newObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        if (newObject != null)
        {
            dbObject.SprintMode = newObject.SprintMode;
            dbObject.Role = newObject.Role;
        }

        await dbContext.SaveChangesAsync();
    }
}
