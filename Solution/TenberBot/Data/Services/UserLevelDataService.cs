using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Data.POCO;

namespace TenberBot.Data.Services;

public interface IUserLevelDataService
{
    Task<UserLevel?> GetByContext(SocketCommandContext context);
    Task<UserLevel?> GetByIds(ulong guildId, ulong userId);

    Task Add(UserLevel newObject);

    Task Update(UserLevel dbObject, UserLevel newObject);

    Task Delete(UserLevel dbObject);

    Task<IList<UserLevel>> GetPage(ulong guildId, LeaderboardView view);

    Task<int> GetUserPage(ulong guildId, ulong userId, LeaderboardView view);

    Task<int> GetCount(ulong guildId, LeaderboardView view);

    Task<UserLevel> LoadVoiceRank(UserLevel dbObject);
    Task<UserLevel> LoadMessageRank(UserLevel dbObject);
    Task<UserLevel> LoadRanks(UserLevel dbObject);

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
            .Include(x => x.ServerUser)
            .FirstOrDefaultAsync(x => x.GuildId == guildId && x.UserId == userId)
            .ConfigureAwait(false);
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

    public async Task<IList<UserLevel>> GetPage(ulong guildId, LeaderboardView view)
    {
        var query = dbContext.UserLevels
            .Include(x => x.ServerUser)
            .Where(x => x.GuildId == guildId);

        if (view.LeaderboardType == LeaderboardType.Message)
            query = query
                .Where(x => x.MessageExperience > view.MinimumExperience)
                .OrderByDescending(x => x.MessageExperience)
                .ThenBy(x => x.UserId);
        else
            query = query
                .Where(x => x.VoiceExperience > view.MinimumExperience)
                .OrderByDescending(x => x.VoiceExperience)
                .ThenBy(x => x.UserId);

        return await query
            .Skip(view.PerPage * view.CurrentPage)
            .Take(view.PerPage)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<int> GetUserPage(ulong guildId, ulong userId, LeaderboardView view)
    {
        var userLevel = await GetByIds(guildId, userId);
        if (userLevel == null)
            return -1;

        if (view.LeaderboardType == LeaderboardType.Message)
        {
            if (userLevel.MessageExperience <= view.MinimumExperience)
                return -1;

            await LoadMessageRank(userLevel);

            return (int)Math.Floor((decimal)userLevel.MessageRank / view.PerPage);
        }
        else
        {
            if (userLevel.VoiceExperience <= view.MinimumExperience)
                return -1;

            await LoadVoiceRank(userLevel);

            return (int)Math.Floor((decimal)userLevel.VoiceRank / view.PerPage);
        }
    }

    public async Task<int> GetCount(ulong guildId, LeaderboardView view)
    {
        var query = dbContext.UserLevels
            .Where(x => x.GuildId == guildId);

        if (view.LeaderboardType == LeaderboardType.Message)
            query = query
                .Where(x => x.MessageExperience > view.MinimumExperience);
        else
            query = query
                .Where(x => x.VoiceExperience > view.MinimumExperience);

        var count = await query
            .CountAsync()
            .ConfigureAwait(false);

        return (int)Math.Ceiling((decimal)count / view.PerPage) - 1;
    }

    public async Task<UserLevel> LoadVoiceRank(UserLevel dbObject)
    {
        var voiceRank = await dbContext.UserLevels
            .Where(x => x.GuildId == dbObject.GuildId)
            .CountAsync(x => x.VoiceExperience > dbObject.VoiceExperience)
            .ConfigureAwait(false);

        dbObject.VoiceRank = voiceRank + 1;

        return dbObject;
    }

    public async Task<UserLevel> LoadMessageRank(UserLevel dbObject)
    {
        var messageRank = await dbContext.UserLevels
            .Where(x => x.GuildId == dbObject.GuildId)
            .CountAsync(x => x.MessageExperience > dbObject.MessageExperience)
            .ConfigureAwait(false);

        dbObject.MessageRank = messageRank + 1;

        return dbObject;
    }

    public async Task<UserLevel> LoadRanks(UserLevel dbObject)
    {
        await LoadVoiceRank(dbObject).ConfigureAwait(false);
        await LoadMessageRank(dbObject).ConfigureAwait(false);

        return dbObject;
    }
}
