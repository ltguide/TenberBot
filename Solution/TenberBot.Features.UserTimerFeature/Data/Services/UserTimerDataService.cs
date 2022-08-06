using Microsoft.EntityFrameworkCore;
using TenberBot.Features.UserTimerFeature.Data.Enums;
using TenberBot.Features.UserTimerFeature.Data.Models;

namespace TenberBot.Features.UserTimerFeature.Data.Services;

public interface IUserTimerDataService
{
    Task<IList<UserTimer>> GetAllActive();

    Task<UserTimer?> GetById(int id);

    Task Add(UserTimer newObject);

    Task Update(UserTimer dbObject, UserTimer newObject);
}

public class UserTimerDataService : IUserTimerDataService
{
    private readonly DataContext dbContext;

    public UserTimerDataService(DataContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IList<UserTimer>> GetAllActive()
    {
        return await dbContext.UserTimers
            .Where(x => x.UserTimerStatus == UserTimerStatus.Started)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<UserTimer?> GetById(int id)
    {
        return await dbContext.UserTimers
            .FirstOrDefaultAsync(x => x.UserTimerId == id)
            .ConfigureAwait(false);
    }

    public async Task Add(UserTimer newObject)
    {
        if (newObject == null)
            throw new ArgumentNullException(nameof(newObject));

        newObject.UserTimerId = 0;

        dbContext.Add(newObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Update(UserTimer dbObject, UserTimer newObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        if (newObject != null)
        {
            dbObject.UserTimerStatus = newObject.UserTimerStatus;
        }

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
