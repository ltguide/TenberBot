using Microsoft.EntityFrameworkCore;
using TenberBot.Data.Models;

namespace TenberBot.Data.Services;

public interface IUserVoiceChannelDataService
{
    Task<IList<UserVoiceChannel>> GetAllByGuildId(ulong guildId);

    Task<UserVoiceChannel?> GetByIds(ulong channelId, ulong userId);

    Task Add(UserVoiceChannel newObject);

    Task Update(UserVoiceChannel dbObject, UserVoiceChannel newObject);

    Task Delete(UserVoiceChannel dbObject);
}

public class UserVoiceChannelDataService : IUserVoiceChannelDataService
{
    private readonly DataContext dbContext;

    public UserVoiceChannelDataService(DataContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IList<UserVoiceChannel>> GetAllByGuildId(ulong guildId)
    {
        return await dbContext.UserVoiceChannels
            .Where(x => x.GuildId == guildId)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<UserVoiceChannel?> GetByIds(ulong channelId, ulong userId)
    {
        return await dbContext.UserVoiceChannels
            .FirstOrDefaultAsync(x => x.ChannelId == channelId && x.UserId == userId)
            .ConfigureAwait(false);
    }

    public async Task Add(UserVoiceChannel newObject)
    {
        if (newObject == null)
            throw new ArgumentNullException(nameof(newObject));

        newObject.UserVoiceChannelId = 0;

        dbContext.Add(newObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Update(UserVoiceChannel dbObject, UserVoiceChannel newObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        if (newObject != null)
        {
        }

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Delete(UserVoiceChannel dbObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        dbContext.Remove(dbObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
