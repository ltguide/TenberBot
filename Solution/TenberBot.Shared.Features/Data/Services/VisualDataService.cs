using Microsoft.EntityFrameworkCore;
using TenberBot.Shared.Features.Data.Models;

namespace TenberBot.Shared.Features.Data.Services;

public interface IVisualDataService
{
    Task<Visual?> GetRandom(string visualType);

    Task<Visual?> GetById(string visualType, int id);

    Task Add(Visual newObject);

    Task Delete(Visual dbObject);
}

public class VisualDataService : IVisualDataService
{
    private readonly SharedDataContext dbContext;

    public VisualDataService(SharedDataContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<Visual?> GetRandom(string visualType)
    {
        return await dbContext.Visuals
            .Where(x => x.VisualType == visualType)
            .OrderBy(x => Guid.NewGuid())
            .AsNoTracking()
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<Visual?> GetById(string visualType, int id)
    {
        return await dbContext.Visuals
            .Where(x => x.VisualType == visualType)
            .FirstOrDefaultAsync(x => x.VisualId == id)
            .ConfigureAwait(false);
    }

    public async Task Add(Visual newObject)
    {
        if (newObject == null)
            throw new ArgumentNullException(nameof(newObject));

        newObject.VisualId = 0;

        dbContext.Add(newObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Delete(Visual dbObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        dbContext.Remove(dbObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
