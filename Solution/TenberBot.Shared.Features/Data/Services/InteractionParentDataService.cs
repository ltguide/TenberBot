using Microsoft.EntityFrameworkCore;
using TenberBot.Shared.Features.Data.Enums;
using TenberBot.Shared.Features.Data.Models;

namespace TenberBot.Shared.Features.Data.Services;

public interface IInteractionParentDataService
{
    Task<InteractionParent?> GetById(InteractionParentType parentType, ulong channelId, ulong? userId);

    Task<InteractionParent?> GetByMessageId(InteractionParentType parentType, ulong messageId);

    Task Add(InteractionParent newObject);

    Task Update(InteractionParent dbObject, InteractionParent newObject);

    Task Delete(InteractionParent dbObject);

    Task<ulong?> Set(InteractionParent newObject);
}

public class InteractionParentDataService : IInteractionParentDataService
{
    private readonly SharedDataContext dbContext;

    public InteractionParentDataService(SharedDataContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<InteractionParent?> GetById(InteractionParentType parentType, ulong channelId, ulong? userId)
    {
        return await dbContext.InteractionParents
            .FirstOrDefaultAsync(x => x.InteractionParentType == parentType && x.ChannelId == channelId && x.UserId == userId)
            .ConfigureAwait(false);
    }

    public async Task<InteractionParent?> GetByMessageId(InteractionParentType parentType, ulong messageId)
    {
        return await dbContext.InteractionParents
            .FirstOrDefaultAsync(x => x.InteractionParentType == parentType && x.MessageId == messageId)
            .ConfigureAwait(false);
    }

    public async Task Add(InteractionParent newObject)
    {
        if (newObject == null)
            throw new ArgumentNullException(nameof(newObject));

        newObject.InteractionParentId = 0;

        dbContext.Add(newObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Update(InteractionParent dbObject, InteractionParent newObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        if (newObject != null)
        {
            dbObject.Update(newObject);
        }

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Delete(InteractionParent dbObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        dbContext.Remove(dbObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<ulong?> Set(InteractionParent newObject)
    {
        ulong? previousParent = null;

        var dbObject = await GetById(newObject.InteractionParentType, newObject.ChannelId, newObject.UserId);

        if (dbObject != null)
        {
            previousParent = dbObject.MessageId;

            await Update(dbObject, newObject);
        }
        else
            await Add(newObject);

        return previousParent;
    }
}
