using Discord;
using Microsoft.EntityFrameworkCore;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Extensions;

namespace TenberBot.Data.Services;

public interface ISprintSnippetDataService
{
    Task<IList<SprintSnippet>> GetAll(SprintSnippetType sprintSnippetType);

    Task<Embed> GetAllAsEmbed(SprintSnippetType sprintSnippetType);

    Task<SprintSnippet?> GetRandom(SprintSnippetType sprintSnippetType);

    Task<SprintSnippet?> GetById(SprintSnippetType sprintSnippetType, int id);

    Task Add(SprintSnippet newObject);

    Task Delete(SprintSnippet dbObject);
}

public class SprintSnippetDataService : ISprintSnippetDataService
{
    private readonly DataContext dbContext;

    public SprintSnippetDataService(DataContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IList<SprintSnippet>> GetAll(SprintSnippetType sprintSnippetType)
    {
        return await dbContext.SprintSnippets
            .Where(x => x.SprintSnippetType == sprintSnippetType)
            .OrderBy(x => x.Text)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<Embed> GetAllAsEmbed(SprintSnippetType sprintSnippetType)
    {
        var lines = (await GetAll(sprintSnippetType)).Select(x => $"`{x.SprintSnippetId,4}` {x.Text.SanitizeMD()}");

        var embedBuilder = new EmbedBuilder
        {
            Title = $"Sprint Snippet: {sprintSnippetType}",
            Color = Color.Blue,
            Description = $"**`  Id` Text**\n{string.Join("\n", lines)}",
        };

        return embedBuilder.Build();
    }

    public async Task<SprintSnippet?> GetRandom(SprintSnippetType sprintSnippetType)
    {
        return await dbContext.SprintSnippets
            .Where(x => x.SprintSnippetType == sprintSnippetType)
            .OrderBy(x => Guid.NewGuid())
            .AsNoTracking()
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<SprintSnippet?> GetById(SprintSnippetType sprintSnippetType, int id)
    {
        return await dbContext.SprintSnippets
            .Where(x => x.SprintSnippetType == sprintSnippetType)
            .FirstOrDefaultAsync(x => x.SprintSnippetId == id)
            .ConfigureAwait(false);
    }

    public async Task Add(SprintSnippet newObject)
    {
        if (newObject == null)
            throw new ArgumentNullException(nameof(newObject));

        newObject.SprintSnippetId = 0;

        dbContext.Add(newObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Delete(SprintSnippet dbObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        dbContext.Remove(dbObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
