using Microsoft.EntityFrameworkCore;
using TenberBot.Data.Models;

namespace TenberBot.Data.Services;

public interface IGlobalSettingDataService
{
    Task<IReadOnlyList<GlobalSetting>> GetAllRO();

    Task<IList<GlobalSetting>> GetAll();
}

public class GlobalSettingDataService : IGlobalSettingDataService
{
    private readonly DataContext dbContext;

    public GlobalSettingDataService(DataContext dbContext)
    {
        this.dbContext = dbContext;
        Console.WriteLine("GlobalSettingDataService hola");
    }

    public async Task<IReadOnlyList<GlobalSetting>> GetAllRO()
    {
        return (await dbContext.Settings
            .ToListAsync()
            .ConfigureAwait(false))
            .AsReadOnly();
    }

    public async Task<IList<GlobalSetting>> GetAll()
    {
        return await dbContext.Settings
            .ToListAsync()
            .ConfigureAwait(false);
    }
}
