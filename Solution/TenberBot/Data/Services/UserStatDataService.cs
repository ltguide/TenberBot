using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using TenberBot.Data.Models;

namespace TenberBot.Data.Services;

public interface IUserStatDataService
{
    Task<UserStat?> GetById(SocketCommandContext context);
    Task<UserStat?> GetById(ulong guildId, ulong userId);

    Task<UserStat> GetOrAddById(SocketCommandContext context);
    Task<UserStat> GetOrAddById(ulong guildId, ulong userId);

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

    public Task<UserStat?> GetById(SocketCommandContext context)
    {
        return GetById(context.Guild.Id, context.User.Id);
    }

    public async Task<UserStat?> GetById(ulong guildId, ulong userId)
    {
        return await dbContext.UserStats
            .FirstOrDefaultAsync(x => x.GuildId == guildId && x.UserId == userId)
            .ConfigureAwait(false);
    }

    public Task<UserStat> GetOrAddById(SocketCommandContext context)
    {
        return GetOrAddById(context.Guild.Id, context.User.Id);
    }

    public async Task<UserStat> GetOrAddById(ulong guildId, ulong userId)
    {
        var dbObject = await GetById(userId, guildId);

        if (dbObject == null)
        {
            dbObject = new UserStat { UserId = userId, GuildId = guildId };

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

        await Save();
    }

    public async Task Delete(UserStat dbObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        dbContext.Remove(dbObject);

        await Save();
    }

    public async Task Save()
    {
        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
