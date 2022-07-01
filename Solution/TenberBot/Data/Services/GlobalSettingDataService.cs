using Microsoft.EntityFrameworkCore;
using TenberBot.Data.Models;

namespace TenberBot.Data.Services;

public interface IGlobalSettingDataService
{
    Task<IList<GlobalSetting>> GetAll();

    Task<GlobalSetting?> GetByName(string name);

    Task Set(string name, string value);
}

public class GlobalSettingDataService : IGlobalSettingDataService
{
    private readonly DataContext dbContext;

    public GlobalSettingDataService(DataContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IList<GlobalSetting>> GetAll()
    {
        return await dbContext.GlobalSettings
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<GlobalSetting?> GetByName(string name)
    {
        return await dbContext.GlobalSettings
            .FirstOrDefaultAsync(x => x.Name == name)
            .ConfigureAwait(false);
    }

    public async Task Set(string name, string value)
    {
        var setting = await GetByName(name);
        if (setting == null)
            dbContext.Add(new GlobalSetting { Name = name, Value = value, });
        else
            setting.Value = value;

        await dbContext.SaveChangesAsync();
    }
}
