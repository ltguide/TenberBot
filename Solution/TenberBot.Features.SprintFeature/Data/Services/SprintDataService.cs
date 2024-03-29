﻿using Microsoft.EntityFrameworkCore;
using TenberBot.Features.SprintFeature.Data.Enums;
using TenberBot.Features.SprintFeature.Data.Models;

namespace TenberBot.Features.SprintFeature.Data.Services;

public interface ISprintDataService
{
    Task<IList<Sprint>> GetAllActive();

    Task<Sprint?> GetById(int id);

    Task Add(Sprint newObject);

    Task Update(Sprint dbObject, Sprint newObject);


    Task<UserSprint?> GetUserById(ulong userId, bool active);
}

public class SprintDataService : ISprintDataService
{
    private readonly DataContext dbContext;

    public SprintDataService(DataContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IList<Sprint>> GetAllActive()
    {
        return await dbContext.Sprints
            .Include(x => x.Users)
            .Where(x => x.SprintStatus == SprintStatus.Waiting || x.SprintStatus == SprintStatus.Started)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<Sprint?> GetById(int id)
    {
        return await dbContext.Sprints
            .Include(x => x.Users)
            .FirstOrDefaultAsync(x => x.SprintId == id)
            .ConfigureAwait(false);
    }

    public async Task Add(Sprint newObject)
    {
        if (newObject == null)
            throw new ArgumentNullException(nameof(newObject));

        newObject.SprintId = 0;

        dbContext.Add(newObject);

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task Update(Sprint dbObject, Sprint newObject)
    {
        if (dbObject == null)
            throw new ArgumentNullException(nameof(dbObject));

        if (newObject != null)
        {
            dbObject.SprintStatus = newObject.SprintStatus;
        }

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<UserSprint?> GetUserById(ulong userId, bool active)
    {
        return await dbContext.UserSprints
            .Include(x => x.Sprint)
            .ThenInclude(x => x.Users)
            .Where(x => x.Sprint.SprintStatus == SprintStatus.Waiting || x.Sprint.SprintStatus == SprintStatus.Started)
            .FirstOrDefaultAsync(x => x.UserId == userId)
            .ConfigureAwait(false);
    }
}
