using Microsoft.EntityFrameworkCore;
using TenberBot.Features.MessageTimerFeature.Data.Enums;
using TenberBot.Features.MessageTimerFeature.Data.Models;

namespace TenberBot.Features.MessageTimerFeature.Data.Services;

public interface IMessageTimerDataService
{
    Task<IList<MessageTimer>> GetAllActive();

    Task<MessageTimer?> GetById(int id);

    Task Add(MessageTimer newObject);

    Task Update(MessageTimer dbObject, MessageTimer newObject);
}

public class MessageTimerDataService : IMessageTimerDataService
{
    private readonly DataContext dbContext;

    public MessageTimerDataService(DataContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IList<MessageTimer>> GetAllActive()
    {
        return await dbContext.MessageTimers
            .Where(x => x.MessageTimerStatus == MessageTimerStatus.Started)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<MessageTimer?> GetById(int id)
    {
        return await dbContext.MessageTimers
            .FirstOrDefaultAsync(x => x.MessageTimerId == id)
            .ConfigureAwait(false);
    }

    public async Task Add(MessageTimer newObject)
    {
        if (newObject == null)
            throw new ArgumentNullException(nameof(newObject));

        newObject.MessageTimerId = 0;

        dbContext.Add(newObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Update(MessageTimer dbObject, MessageTimer newObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        if (newObject != null)
        {
            dbObject.MessageTimerStatus = newObject.MessageTimerStatus;
        }

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
