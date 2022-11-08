using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using TenberBot.Features.ExperienceFeature.Data.Enums;
using TenberBot.Features.ExperienceFeature.Data.Models;
using TenberBot.Features.ExperienceFeature.Data.POCO;

namespace TenberBot.Features.ExperienceFeature.Data.Services;

public interface IUserLevelDataService
{
    Task<UserLevel?> GetByContext(SocketCommandContext context);
    Task<UserLevel?> GetByIds(ulong guildId, ulong userId);

    Task Add(UserLevel newObject);

    Task Update(UserLevel dbObject, UserLevel newObject);

    Task Delete(UserLevel dbObject);

    Task ResetEventAExperience(ulong guildId);
    Task ResetEventBExperience(ulong guildId);

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

    public async Task ResetEventAExperience(ulong guildId)
    {
        var dbObjects = await dbContext.UserLevels
            .Where(x => x.GuildId == guildId)
            .Where(x => x.EventAExperience > 0)
            .ToListAsync()
            .ConfigureAwait(false);

        foreach (var dbObject in dbObjects)
            dbObject.EventAExperience = 0;

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task ResetEventBExperience(ulong guildId)
    {
        var dbObjects = await dbContext.UserLevels
            .Where(x => x.GuildId == guildId)
            .Where(x => x.EventBExperience > 0)
            .ToListAsync()
            .ConfigureAwait(false);

        foreach (var dbObject in dbObjects)
            dbObject.EventBExperience = 0;

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<IList<UserLevel>> GetPage(ulong guildId, LeaderboardView view)
    {
        var query = dbContext.UserLevels
            .Include(x => x.ServerUser)
            .Where(x => x.GuildId == guildId);

        switch (view.LeaderboardType)
        {
            case LeaderboardType.Message:
                query = query
                    .Where(x => x.MessageExperience >= view.MinimumExperience)
                    .OrderByDescending(x => x.MessageExperience)
                    .ThenBy(x => x.UserId);
                break;

            case LeaderboardType.Voice:
                query = query
                    .Where(x => x.VoiceExperience >= view.MinimumExperience)
                    .OrderByDescending(x => x.VoiceExperience)
                    .ThenBy(x => x.UserId);
                break;

            case LeaderboardType.EventA:
                query = query
                    .Where(x => x.EventAExperience >= view.MinimumExperience)
                    .OrderByDescending(x => x.EventAExperience)
                    .ThenBy(x => x.UserId);
                break;

            case LeaderboardType.EventB:
                query = query
                    .Where(x => x.EventBExperience >= view.MinimumExperience)
                    .OrderByDescending(x => x.EventBExperience)
                    .ThenBy(x => x.UserId);
                break;
        }

        return await query
            .Skip(view.PerPage * view.CurrentPage)
            .Take(view.PerPage)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<int> GetUserPage(ulong guildId, ulong userId, LeaderboardView view)
    {
        var userLevel = await GetByIds(guildId, userId);
        if (userLevel == null)
            return -1;

        switch (view.LeaderboardType)
        {
            case LeaderboardType.Message:
                if (userLevel.MessageExperience <= view.MinimumExperience)
                    return -1;

                await LoadMessageRank(userLevel);

                return (int)Math.Floor((decimal)(userLevel.MessageRank - 1) / view.PerPage);

            case LeaderboardType.Voice:
                if (userLevel.VoiceExperience <= view.MinimumExperience)
                    return -1;

                await LoadVoiceRank(userLevel);

                return (int)Math.Floor((decimal)(userLevel.VoiceRank - 1) / view.PerPage);

            case LeaderboardType.EventA:
                if (userLevel.EventAExperience <= view.MinimumExperience)
                    return -1;

                await LoadEventARank(userLevel);

                return (int)Math.Floor((decimal)(userLevel.EventRank - 1) / view.PerPage);

            case LeaderboardType.EventB:
                if (userLevel.EventBExperience <= view.MinimumExperience)
                    return -1;

                await LoadEventBRank(userLevel);

                return (int)Math.Floor((decimal)(userLevel.EventRank - 1) / view.PerPage);

            default:
                return -1;
        }
    }

    public async Task<int> GetCount(ulong guildId, LeaderboardView view)
    {
        var query = dbContext.UserLevels
            .Where(x => x.GuildId == guildId);

        switch (view.LeaderboardType)
        {
            case LeaderboardType.Message:
                query = query.Where(x => x.MessageExperience > view.MinimumExperience);
                break;

            case LeaderboardType.Voice:
                query = query.Where(x => x.VoiceExperience > view.MinimumExperience);
                break;

            case LeaderboardType.EventA:
                query = query.Where(x => x.EventAExperience > view.MinimumExperience);
                break;

            case LeaderboardType.EventB:
                query = query.Where(x => x.EventBExperience > view.MinimumExperience);
                break;
        }

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

    public async Task<UserLevel> LoadEventARank(UserLevel dbObject)
    {
        var eventRank = await dbContext.UserLevels
            .Where(x => x.GuildId == dbObject.GuildId)
            .CountAsync(x => x.EventAExperience > dbObject.EventAExperience)
            .ConfigureAwait(false);

        dbObject.EventRank = eventRank + 1;

        return dbObject;
    }

    public async Task<UserLevel> LoadEventBRank(UserLevel dbObject)
    {
        var eventRank = await dbContext.UserLevels
            .Where(x => x.GuildId == dbObject.GuildId)
            .CountAsync(x => x.EventBExperience > dbObject.EventBExperience)
            .ConfigureAwait(false);

        dbObject.EventRank = eventRank + 1;

        return dbObject;
    }

    public async Task<UserLevel> LoadRanks(UserLevel dbObject)
    {
        await LoadVoiceRank(dbObject).ConfigureAwait(false);
        await LoadMessageRank(dbObject).ConfigureAwait(false);

        return dbObject;
    }
}
