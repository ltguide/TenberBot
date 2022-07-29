using Discord;
using Discord.Interactions;
using TenberBot.Data.Models;
using TenberBot.Data.Services;
using TenberBot.Extensions;

namespace TenberBot.Modules.Interaction;

[Group("event-experience", "Manage event experience.")]
[DefaultMemberPermissions(GuildPermission.ManageChannels)]
public class EventExperienceInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IUserLevelDataService userLevelDataService;

    public EventExperienceInteractionModule(
        IUserLevelDataService userLevelDataService)
    {
        this.userLevelDataService = userLevelDataService;
    }

    [SlashCommand("get", "Get event experience for a user.")]
    public async Task Get(IUser user)
    {
        var dbUserLevel = await GetRecord(user);
        if (dbUserLevel == null)
            return;

        await SendExperience(dbUserLevel, null);
    }

    [SlashCommand("modify", "Modify event experience for a user.")]
    public async Task Modify(IUser user, decimal amount)
    {
        var dbUserLevel = await GetRecord(user);
        if (dbUserLevel == null)
            return;

        var before = dbUserLevel.EventExperience;

        dbUserLevel.EventExperience = Math.Max(0, Math.Min(decimal.MaxValue, dbUserLevel.EventExperience + amount));

        await userLevelDataService.Update(dbUserLevel, null!);

        await SendExperience(dbUserLevel, before);
    }

    [SlashCommand("set", "Set event experience for a user.")]
    public async Task Set(IUser user, decimal amount)
    {
        var dbUserLevel = await GetRecord(user);
        if (dbUserLevel == null)
            return;

        var before = dbUserLevel.EventExperience;

        dbUserLevel.EventExperience = Math.Max(0, Math.Min(decimal.MaxValue, amount));

        await userLevelDataService.Update(dbUserLevel, null!);

        await SendExperience(dbUserLevel, before);
    }

    [SlashCommand("reset", "Set event experience for all users to 0.")]
    public async Task Reset(bool confirm)
    {
        if (confirm)
        {
            await userLevelDataService.ResetEventExperience(Context.Guild.Id);

            await RespondAsync($"All event experience has been reset.");
        }
        else
            await RespondAsync("I didn't reset the event experience.", ephemeral: true);
    }

    private async Task<UserLevel?> GetRecord(IUser user)
    {
        var dbUserLevel = await userLevelDataService.GetByIds(Context.Guild.Id, user.Id);
        if (dbUserLevel == null)
            await RespondAsync($"I don't have an experience record for {user.Mention}", ephemeral: true);

        return dbUserLevel;
    }

    private Task SendExperience(UserLevel userLevel, decimal? before)
    {
        var previous = before != null ? $" It used to be {before:N2}." : "";

        return RespondAsync($"{userLevel.UserId.GetUserMention()} has {userLevel.EventExperience:N2} event experience.{previous}", allowedMentions: AllowedMentions.None);
    }
}
