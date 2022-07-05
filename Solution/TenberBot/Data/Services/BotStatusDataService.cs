using Discord;
using Microsoft.EntityFrameworkCore;
using TenberBot.Data.Models;

namespace TenberBot.Data.Services;

public interface IBotStatusDataService
{
    Task<IList<BotStatus>> GetAll();

    Task<Embed> GetAllAsEmbed();

    Task<BotStatus?> GetRandom();

    Task<BotStatus?> GetById(int id);

    Task Add(BotStatus newObject);

    Task Delete(BotStatus dbObject);
}

public class BotStatusDataService : IBotStatusDataService
{
    private readonly DataContext dbContext;

    public BotStatusDataService(DataContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IList<BotStatus>> GetAll()
    {
        return await dbContext.BotStatuses
            .OrderBy(x => x.Text)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<Embed> GetAllAsEmbed()
    {
        var lines = (await GetAll()).Select(x => $"`{x.BotStatusId,4}` {x.Text}");

        var embedBuilder = new EmbedBuilder
        {
            Title = "Bot Statuses",
            Color = Color.Blue,
            Description = $"**`  Id` Text**\n{string.Join("\n", lines)}",
        };

        return embedBuilder.Build();
    }

    public async Task<BotStatus?> GetRandom()
    {
        return await dbContext.BotStatuses
            .OrderBy(x => Guid.NewGuid())
            .AsNoTracking()
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<BotStatus?> GetById(int id)
    {
        return await dbContext.BotStatuses
            .FirstOrDefaultAsync(x => x.BotStatusId == id)
            .ConfigureAwait(false);
    }

    public async Task Add(BotStatus newObject)
    {
        if (newObject == null)
            throw new ArgumentNullException(nameof(newObject));

        newObject.BotStatusId = 0;

        dbContext.Add(newObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Delete(BotStatus dbObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        dbContext.Remove(dbObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
