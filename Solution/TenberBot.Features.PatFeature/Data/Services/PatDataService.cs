using Discord;
using Microsoft.EntityFrameworkCore;
using TenberBot.Features.PatFeature.Data.Enums;
using TenberBot.Features.PatFeature.Data.Models;
using TenberBot.Shared.Features.Extensions.Strings;

namespace TenberBot.Features.PatFeature.Data.Services;

public interface IPatDataService
{
    Task<IList<Pat>> GetAll(PatType patType);

    Task<Embed> GetAllAsEmbed(PatType patType);

    Task<Pat?> GetRandom(PatType patType);

    Task<Pat?> GetById(PatType patType, int id);

    Task Add(Pat newObject);

    Task Delete(Pat dbObject);
}

public class PatDataService : IPatDataService
{
    private readonly DataContext dbContext;

    public PatDataService(DataContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IList<Pat>> GetAll(PatType patType)
    {
        return await dbContext.Pats
            .Where(x => x.PatType == patType)
            .OrderBy(x => x.Text)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<Embed> GetAllAsEmbed(PatType patType)
    {
        var lines = (await GetAll(patType)).Select(x => $"`{x.PatId,4}` {x.Text.SanitizeMD()}");

        var embedBuilder = new EmbedBuilder
        {
            Title = $"Pat: {patType}",
            Color = Color.Blue,
            Description = $"**`  Id` Text**\n{string.Join("\n", lines)}",
        };

        return embedBuilder.Build();
    }

    public async Task<Pat?> GetRandom(PatType patType)
    {
        return await dbContext.Pats
            .Where(x => x.PatType == patType)
            .OrderBy(x => Guid.NewGuid())
            .AsNoTracking()
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<Pat?> GetById(PatType patType, int id)
    {
        return await dbContext.Pats
            .Where(x => x.PatType == patType)
            .FirstOrDefaultAsync(x => x.PatId == id)
            .ConfigureAwait(false);
    }

    public async Task Add(Pat newObject)
    {
        if (newObject == null)
            throw new ArgumentNullException(nameof(newObject));

        newObject.PatId = 0;

        dbContext.Add(newObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Delete(Pat dbObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        dbContext.Remove(dbObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
