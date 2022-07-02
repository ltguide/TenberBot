using Microsoft.EntityFrameworkCore;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;

namespace TenberBot.Data.Services;

public interface IVisualDataService
{
    Task<Visual?> GetRandom(VisualType visualType);

    Task<Visual?> GetById(VisualType visualType, int id);

    Task Add(Visual newObject);

    Task Delete(Visual dbObject);
}

public class VisualDataService : IVisualDataService
{
    private readonly DataContext dbContext;

    public VisualDataService(DataContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<Visual?> GetRandom(VisualType visualType)
    {
        return await dbContext.Visuals
            .Where(x => x.VisualType == visualType)
            .OrderBy(x => Guid.NewGuid())
            .AsNoTracking()
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);
    }

    public async Task<Visual?> GetById(VisualType visualType, int id)
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
