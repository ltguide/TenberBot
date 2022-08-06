using Discord;
using Discord.Interactions;
using TenberBot.Features.ExperienceFeature.Data.Models;
using TenberBot.Features.ExperienceFeature.Services;
using TenberBot.Shared.Features.Extensions.Mentions;
using TenberBot.Shared.Features.Extensions.Strings;

namespace TenberBot.Features.ExperienceFeature.Modules.Interaction;

[Group("event-experience", "Manage event experience.")]
[DefaultMemberPermissions(GuildPermission.ManageChannels)]
public class EventExperienceInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly GuildExperienceService guildExperienceService;

    public EventExperienceInteractionModule(
        GuildExperienceService guildExperienceService)
    {
        this.guildExperienceService = guildExperienceService;
    }

    [SlashCommand("get", "Get event experience for a user.")]
    public async Task Get(IUser user)
    {
        var dbUserLevel = await GetRecord(user);
        if (dbUserLevel == null)
            return;

        await SendExperience(dbUserLevel, null, null);
    }

    [SlashCommand("modify", "Modify event experience for a user.")]
    public async Task Modify(IUser user, decimal amount, string? comment = null)
    {
        var dbUserLevel = await GetRecord(user);
        if (dbUserLevel == null)
            return;

        var before = dbUserLevel.EventExperience;

        await guildExperienceService.SetEventExperience(dbUserLevel, Math.Max(0, Math.Min(decimal.MaxValue, dbUserLevel.EventExperience + amount)));

        await SendExperience(dbUserLevel, before, comment);
    }

    [SlashCommand("set", "Set event experience for a user.")]
    public async Task Set(IUser user, decimal amount, string? comment = null)
    {
        var dbUserLevel = await GetRecord(user);
        if (dbUserLevel == null)
            return;

        var before = dbUserLevel.EventExperience;

        await guildExperienceService.SetEventExperience(dbUserLevel, Math.Max(0, Math.Min(decimal.MaxValue, amount)));

        await SendExperience(dbUserLevel, before, comment);
    }

    [SlashCommand("reset", "Set event experience for all users to 0.")]
    public async Task Reset(bool confirm)
    {
        if (confirm)
        {
            await guildExperienceService.ResetEventExperience(Context.Guild.Id);

            await RespondAsync($"All event experience has been reset.");
        }
        else
            await RespondAsync("I didn't reset the event experience.", ephemeral: true);
    }

    private async Task<UserLevel?> GetRecord(IUser user)
    {
        var dbUserLevel = await guildExperienceService.GetUserLevel(Context.Guild.Id, user.Id);
        if (dbUserLevel == null)
            await RespondAsync($"I don't have an experience record for {user.Mention}", ephemeral: true);

        return dbUserLevel;
    }

    private Task SendExperience(UserLevel userLevel, decimal? before, string? comment)
    {
        var beforeText = before != null ? $" It used to be {before:N2}." : "";

        var commentText = comment != null ? $"\nComment: {comment.SanitizeMD()}" : "";

        return RespondAsync($"{userLevel.UserId.GetUserMention()} has {userLevel.EventExperience:N2} event experience.{beforeText}{commentText}", allowedMentions: AllowedMentions.None);
    }
}
