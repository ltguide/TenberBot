using Discord;
using Microsoft.EntityFrameworkCore;
using TenberBot.Features.HighlightFeature.Data.Enums;
using TenberBot.Features.HighlightFeature.Data.Models;
using TenberBot.Shared.Features.Extensions.Strings;

namespace TenberBot.Features.HighlightFeature.Data.Services;

public interface IHighlightDataService
{
    Task<IList<HighFive>> GetAll(HighFiveType highFiveType);

    Task<Embed> GetAllAsEmbed(HighFiveType highFiveType);

    Task<HighFive?> GetRandom(HighFiveType highFiveType);

    Task<HighFive?> GetById(HighFiveType highFiveType, int id);

    Task Add(HighFive newObject);

    Task Delete(HighFive dbObject);
}

public class HighlightDataService : IHighlightDataService
{
    private readonly DataContext dbContext;

    public HighlightDataService(DataContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IList<HighFive>> GetAll(HighFiveType highFiveType)
    {
        return await dbContext.HighFives
            .Where(x => x.HighFiveType == highFiveType)
            .OrderBy(x => x.Text)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<Embed> GetAllAsEmbed(HighFiveType highFiveType)
    {
        var lines = (await GetAll(highFiveType)).Select(x => $"`{x.HighFiveId,4}` {x.Text.SanitizeMD()}");

        var embedBuilder = new EmbedBuilder
        {
            Title = $"High Five: {highFiveType}",
            Color = Color.Blue,
            Description = $"**`  Id` Text**\n{string.Join("\n", lines)}",
        };

        return embedBuilder.Build();
    }

    public async Task<HighFive?> GetRandom(HighFiveType highFiveType)
    {
        return await dbContext.HighFives
            .Where(x => x.HighFiveType == highFiveType)
            .OrderBy(x => Guid.NewGuid())
            .AsNoTracking()
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<HighFive?> GetById(HighFiveType highFiveType, int id)
    {
        return await dbContext.HighFives
            .Where(x => x.HighFiveType == highFiveType)
            .FirstOrDefaultAsync(x => x.HighFiveId == id)
            .ConfigureAwait(false);
    }

    public async Task Add(HighFive newObject)
    {
        if (newObject == null)
            throw new ArgumentNullException(nameof(newObject));

        newObject.HighFiveId = 0;

        dbContext.Add(newObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Delete(HighFive dbObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        dbContext.Remove(dbObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
