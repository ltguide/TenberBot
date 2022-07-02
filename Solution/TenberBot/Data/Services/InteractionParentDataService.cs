using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using TenberBot.Data.Models;
using TenberBot.Data.Enums;
using TenberBot.Extensions;

namespace TenberBot.Data.Services;

public interface IInteractionParentDataService
{
    Task<InteractionParent?> GetByContext(InteractionParentType parentType, SocketCommandContext context);
    Task<InteractionParent?> GetByIds(InteractionParentType parentType, ulong guildId, ulong channelId, ulong? userId);

    Task Add(InteractionParent newObject);

    Task Update(InteractionParent dbObject, ulong messageId, ulong userId);

    Task Delete(InteractionParent dbObject);
}

public class InteractionParentDataService : IInteractionParentDataService
{
    private readonly DataContext dbContext;

    public InteractionParentDataService(DataContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<InteractionParent?> GetByContext(InteractionParentType parentType, SocketCommandContext context)
    {
        return GetByIds(parentType, context.Guild.Id, context.Channel.Id, context.User.Id);
    }

    public async Task<InteractionParent?> GetByIds(InteractionParentType parentType, ulong guildId, ulong channelId, ulong? userId)
    {
        var query = dbContext.InteractionParents
            .Where(x => x.InteractionParentType == parentType && x.GuildId == guildId && x.ChannelId == channelId);

        if (parentType.GetLink() == InteractionParentLink.ChannelUser)
            query = query.Where(x => x.UserId == userId);

        return await query
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task Add(InteractionParent newObject)
    {
        if (newObject == null)
            throw new ArgumentNullException(nameof(newObject));

        newObject.InteractionParentId = 0;

        dbContext.Add(newObject);

        await dbContext.SaveChangesAsync();
    }

    public async Task Update(InteractionParent dbObject, ulong messageId, ulong userId)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        dbObject.MessageId = messageId;
        dbObject.UserId = userId;

        await dbContext.SaveChangesAsync();
    }

    public async Task Delete(InteractionParent dbObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        dbContext.Remove(dbObject);

        await dbContext.SaveChangesAsync();
    }
}
