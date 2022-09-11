using Discord;
using Microsoft.EntityFrameworkCore;
using TenberBot.Features.FortuneFeature.Data.Models;
using TenberBot.Shared.Features.Extensions.Strings;

namespace TenberBot.Features.FortuneFeature.Data.Services;

public interface IFortuneDataService
{
    Task<IList<Fortune>> GetAll();

    Task<Embed> GetAllAsEmbed();

    Task<Fortune?> GetRandom();

    Task<Fortune?> GetById(int id);

    Task Add(Fortune newObject);

    Task Delete(Fortune dbObject);
}

public class FortuneDataService : IFortuneDataService
{
    private readonly DataContext dbContext;

    public FortuneDataService(DataContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IList<Fortune>> GetAll()
    {
        return await dbContext.Fortunes
            .OrderBy(x => x.Text)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<Embed> GetAllAsEmbed()
    {
        var lines = (await GetAll()).Select(x => $"`{x.FortuneId,4}` {x.Text.SanitizeMD()}");

        var embedBuilder = new EmbedBuilder
        {
            Title = $"Fortunes",
            Color = Color.Blue,
            Description = $"**`  Id` Text**\n{string.Join("\n", lines)}",
        };

        return embedBuilder.Build();
    }

    public async Task<Fortune?> GetRandom()
    {
        return await dbContext.Fortunes
            .OrderBy(x => Guid.NewGuid())
            .AsNoTracking()
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<Fortune?> GetById(int id)
    {
        return await dbContext.Fortunes
            .FirstOrDefaultAsync(x => x.FortuneId == id)
            .ConfigureAwait(false);
    }

    public async Task Add(Fortune newObject)
    {
        if (newObject == null)
            throw new ArgumentNullException(nameof(newObject));

        newObject.FortuneId = 0;

        dbContext.Add(newObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Delete(Fortune dbObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        dbContext.Remove(dbObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
