using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using TenberBot.Features.HighlightFeature.Data.Models;
using TenberBot.Features.HighlightFeature.Data.POCO;

namespace TenberBot.Features.HighlightFeature.Data.Services;

public interface IHighlightWordDataService
{
    Task<IList<HighlightWord>> GetAll();

    Task<HighlightWord?> Get(HighlightWord newObject);

    Task<HighlightWord?> GetByIndex(ulong guildId, ulong userId, int index);

    Task Add(HighlightWord newObject);

    Task Delete(HighlightWord dbObject);

    Task<IList<HighlightWord>> GetPage(ulong guildId, ulong userId, WordsView view);

    Task<int> GetCount(ulong guildId, ulong userId, WordsView view);
}

public class HighlightWordDataService : IHighlightWordDataService
{
    private readonly DataContext dbContext;

    public HighlightWordDataService(DataContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IList<HighlightWord>> GetAll()
    {
        return await dbContext.HighlightWords
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<HighlightWord?> Get(HighlightWord newObject)
    {
        return await dbContext.HighlightWords
            .FirstOrDefaultAsync(x => x.GuildId == newObject.GuildId && x.UserId == newObject.UserId && x.Word == newObject.Word)
            .ConfigureAwait(false);
    }

    [SuppressMessage("Performance", "CA1845:Use span-based 'string.Concat'", Justification = "ReadOnlySpan cant be used in expression tree")]
    public async Task<HighlightWord?> GetByIndex(ulong guildId, ulong userId, int index)
    {
        if (index < 1)
            return null;

        return await dbContext.HighlightWords
            .Where(x => x.GuildId == guildId && x.UserId == userId)
            .OrderBy(x => x.Word.Substring(0, 1).Replace("*", "") + x.Word.Substring(1))
            .ThenBy(x => x.HighlightWordId)
            .Skip(index - 1)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task Add(HighlightWord newObject)
    {
        if (newObject == null)
            throw new ArgumentNullException(nameof(newObject));

        newObject.HighlightWordId = 0;

        dbContext.Add(newObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Delete(HighlightWord dbObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        dbContext.Remove(dbObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    [SuppressMessage("Performance", "CA1845:Use span-based 'string.Concat'", Justification = "ReadOnlySpan cant be used in expression tree")]
    public async Task<IList<HighlightWord>> GetPage(ulong guildId, ulong userId, WordsView view)
    {
        return await dbContext.HighlightWords
            .Where(x => x.GuildId == guildId && x.UserId == userId)
            .OrderBy(x => x.Word.Substring(0, 1).Replace("*", "") + x.Word.Substring(1))
            .ThenBy(x => x.HighlightWordId)
            .Skip(view.PerPage * view.CurrentPage)
            .Take(view.PerPage)
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<int> GetCount(ulong guildId, ulong userId, WordsView view)
    {
        var count = await dbContext.HighlightWords
            .Where(x => x.GuildId == guildId && x.UserId == userId)
            .CountAsync()
            .ConfigureAwait(false);

        return (int)Math.Ceiling((decimal)count / view.PerPage) - 1;
    }
}
