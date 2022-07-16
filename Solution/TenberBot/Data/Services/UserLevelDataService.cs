using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;

namespace TenberBot.Data.Services;

public interface IUserLevelDataService
{
    Task<UserLevel?> GetByContext(SocketCommandContext context);
    Task<UserLevel?> GetByIds(ulong guildId, ulong userId);

    Task<IList<UserLevel>> GetPage(ulong guildId, int perPage, int pageNumber, LeaderboardType leaderboardType);

    Task<int> GetCount(ulong guildId, int perPage);

    Task<UserLevel> GetRanks(UserLevel dbObject);

    Task Add(UserLevel newObject);

    Task Update(UserLevel dbObject, UserLevel newObject);

    Task Delete(UserLevel dbObject);
}

public class UserLevelDataService : IUserLevelDataService
{
    private readonly DataContext dbContext;

    public UserLevelDataService(DataContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<UserLevel?> GetByContext(SocketCommandContext context)
    {
        return GetByIds(context.Guild.Id, context.User.Id);
    }

    public async Task<UserLevel?> GetByIds(ulong guildId, ulong userId)
    {
        return await dbContext.UserLevels
            .FirstOrDefaultAsync(x => x.GuildId == guildId && x.UserId == userId)
            .ConfigureAwait(false);
    }

    public async Task<IList<UserLevel>> GetPage(ulong guildId, int perPage, int pageNumber, LeaderboardType leaderboardType)
    {
        return await dbContext.UserLevels
            .Where(x => x.GuildId == guildId)
            .OrderByDescending(x => leaderboardType == LeaderboardType.Message ? x.MessageExperience : x.VoiceExperience)
            .ThenBy(x => x.UserId)
            .Skip(perPage * pageNumber)
            .Take(perPage)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<int> GetCount(ulong guildId, int perPage)
    {
        var count = await dbContext.UserLevels
            .Where(x => x.GuildId == guildId)
            .CountAsync()
            .ConfigureAwait(false);

        return (int)Math.Ceiling((decimal)count / perPage) - 1;
    }

    public async Task<UserLevel> GetRanks(UserLevel dbObject)
    {
        var voiceRank = await dbContext.UserLevels
            .Where(x => x.GuildId == dbObject.GuildId)
            .CountAsync(x => x.VoiceExperience > dbObject.VoiceExperience)
            .ConfigureAwait(false);

        dbObject.VoiceRank = voiceRank + 1;

        var messageRank = await dbContext.UserLevels
            .Where(x => x.GuildId == dbObject.GuildId)
            .CountAsync(x => x.MessageExperience > dbObject.MessageExperience)
            .ConfigureAwait(false);

        dbObject.MessageRank = messageRank + 1;

        return dbObject;
    }

    public async Task Add(UserLevel newObject)
    {
        if (newObject == null)
            throw new ArgumentNullException(nameof(newObject));

        newObject.UserLevelId = 0;

        dbContext.Add(newObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Update(UserLevel dbObject, UserLevel newObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        if (newObject != null)
        {
        }

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Delete(UserLevel dbObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        dbContext.Remove(dbObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
