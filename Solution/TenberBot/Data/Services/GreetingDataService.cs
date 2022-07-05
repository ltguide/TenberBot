using Discord;
using Microsoft.EntityFrameworkCore;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Extensions;

namespace TenberBot.Data.Services;

public interface IGreetingDataService
{
    Task<IList<Greeting>> GetAll(GreetingType greetingType);

    Task<Embed> GetAllAsEmbed(GreetingType greetingType);

    Task<Greeting?> GetRandom(GreetingType greetingType);

    Task<Greeting?> GetById(GreetingType greetingType, int id);

    Task Add(Greeting newObject);

    Task Delete(Greeting dbObject);
}

public class GreetingDataService : IGreetingDataService
{
    private readonly DataContext dbContext;

    public GreetingDataService(DataContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IList<Greeting>> GetAll(GreetingType greetingType)
    {
        return await dbContext.Greetings
            .Where(x => x.GreetingType == greetingType)
            .OrderBy(x => x.Text)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<Embed> GetAllAsEmbed(GreetingType greetingType)
    {
        var lines = (await GetAll(greetingType)).Select(x => $"`{x.GreetingId,4}` {x.Text.SanitizeMD()}");

        var embedBuilder = new EmbedBuilder
        {
            Title = $"Greeting: {greetingType}",
            Color = Color.Blue,
            Description = $"**`  Id` Text**\n{string.Join("\n", lines)}",
        };

        return embedBuilder.Build();
    }

    public async Task<Greeting?> GetRandom(GreetingType greetingType)
    {
        return await dbContext.Greetings
            .Where(x => x.GreetingType == greetingType)
            .OrderBy(x => Guid.NewGuid())
            .AsNoTracking()
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<Greeting?> GetById(GreetingType greetingType, int id)
    {
        return await dbContext.Greetings
            .Where(x => x.GreetingType == greetingType)
            .FirstOrDefaultAsync(x => x.GreetingId == id)
            .ConfigureAwait(false);
    }

    public async Task Add(Greeting newObject)
    {
        if (newObject == null)
            throw new ArgumentNullException(nameof(newObject));

        newObject.GreetingId = 0;

        dbContext.Add(newObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Delete(Greeting dbObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        dbContext.Remove(dbObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
