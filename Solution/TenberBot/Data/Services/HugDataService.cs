using Discord;
using Microsoft.EntityFrameworkCore;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Extensions;

namespace TenberBot.Data.Services;

public interface IHugDataService
{
    Task<IList<Hug>> GetAll(HugType hugType);

    Task<Embed> GetAllAsEmbed(HugType hugType);

    Task<Hug?> GetRandom(HugType hugType);

    Task<Hug?> GetById(HugType hugType, int id);

    Task Add(Hug newObject);

    Task Delete(Hug dbObject);
}

public class HugDataService : IHugDataService
{
    private readonly DataContext dbContext;

    public HugDataService(DataContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IList<Hug>> GetAll(HugType hugType)
    {
        return await dbContext.Hugs
            .Where(x => x.HugType == hugType)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<Embed> GetAllAsEmbed(HugType hugType)
    {
        var lines = (await GetAll(hugType)).Select(x => $"`{x.HugId,4}` {x.Text.SanitizeMD()}");

        var embedBuilder = new EmbedBuilder
        {
            Title = $"Hug: {hugType}",
            Color = Color.Blue,
            Description = $"**`  Id` Text**\n{string.Join("\n", lines)}",
        };

        return embedBuilder.Build();
    }

    public async Task<Hug?> GetRandom(HugType hugType)
    {
        return await dbContext.Hugs
            .Where(x => x.HugType == hugType)
            .OrderBy(x => Guid.NewGuid())
            .AsNoTracking()
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<Hug?> GetById(HugType hugType, int id)
    {
        return await dbContext.Hugs
            .Where(x => x.HugType == hugType)
            .FirstOrDefaultAsync(x => x.HugId == id)
            .ConfigureAwait(false);
    }

    public async Task Add(Hug newObject)
    {
        if (newObject == null)
            throw new ArgumentNullException(nameof(newObject));

        newObject.HugId = 0;

        dbContext.Add(newObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Delete(Hug dbObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        dbContext.Remove(dbObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
