using Discord.Commands;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using TenberBot.Data.Models;

namespace TenberBot.Data.Services;

public interface IUserStatDataService
{
    Task<UserStat?> GetByContext(SocketInteractionContext context);
    Task<UserStat?> GetByContext(SocketCommandContext context);
    Task<UserStat?> GetByIds(ulong guildId, ulong userId);

    Task<UserStat> GetOrAddByContext(SocketInteractionContext context);
    Task<UserStat> GetOrAddByContext(SocketCommandContext context);
    Task<UserStat> GetOrAddByIds(ulong guildId, ulong userId);

    Task Add(UserStat newObject);

    Task Delete(UserStat dbObject);

    Task Save();
}

public class UserStatDataService : IUserStatDataService
{
    private readonly DataContext dbContext;

    public UserStatDataService(DataContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<UserStat?> GetByContext(SocketInteractionContext context)
    {
        return GetByIds(context.Guild.Id, context.User.Id);
    }

    public Task<UserStat?> GetByContext(SocketCommandContext context)
    {
        return GetByIds(context.Guild.Id, context.User.Id);
    }

    public async Task<UserStat?> GetByIds(ulong guildId, ulong userId)
    {
        return await dbContext.UserStats
            .FirstOrDefaultAsync(x => x.GuildId == guildId && x.UserId == userId)
            .ConfigureAwait(false);
    }

    public Task<UserStat> GetOrAddByContext(SocketInteractionContext context)
    {
        return GetOrAddByIds(context.Guild.Id, context.User.Id);
    }

    public Task<UserStat> GetOrAddByContext(SocketCommandContext context)
    {
        return GetOrAddByIds(context.Guild.Id, context.User.Id);
    }

    public async Task<UserStat> GetOrAddByIds(ulong guildId, ulong userId)
    {
        var dbObject = await GetByIds(guildId, userId);

        if (dbObject == null)
        {
            dbObject = new UserStat { GuildId = guildId, UserId = userId };

            await Add(dbObject);
        }

        return dbObject;
    }

    public async Task Add(UserStat newObject)
    {
        if (newObject == null)
            throw new ArgumentNullException(nameof(newObject));

        newObject.UserStatId = 0;

        dbContext.Add(newObject);

        await dbContext.SaveChangesAsync();
    }

    public async Task Delete(UserStat dbObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        dbContext.Remove(dbObject);

        await dbContext.SaveChangesAsync();
    }

    public async Task Save()
    {
        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
