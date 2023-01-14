using Discord;
using Discord.Interactions;
using TenberBot.Features.ExperienceFeature.Data.Models;
using TenberBot.Features.ExperienceFeature.Services;
using TenberBot.Shared.Features.Attributes.Modules;
using TenberBot.Shared.Features.Extensions.Mentions;
using TenberBot.Shared.Features.Extensions.Strings;

namespace TenberBot.Features.ExperienceFeature.Modules.Interaction;

[Group("event-experience", "Manage event experience.")]
[HelpCommand(group: "Server Management")]
[DefaultMemberPermissions(GuildPermission.ManageChannels)]
[EnabledInDm(false)]
public class EventExperienceInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly GuildExperienceService guildExperienceService;

    public EventExperienceInteractionModule(
        GuildExperienceService guildExperienceService)
    {
        this.guildExperienceService = guildExperienceService;
    }

    [SlashCommand("get", "Get event experience for a user.")]
    [HelpCommand("`<event>` `<user>`")]
    public async Task Get(
        [Summary("event-name")][Choice("event-a", "EventA"), Choice("event-b", "EventB")] string eventName,
        IUser user)
    {
        var dbUserLevel = await GetRecord(user);
        if (dbUserLevel == null)
            return;

        await SendExperience(eventName, dbUserLevel, null, null);
    }

    [SlashCommand("modify", "Modify event experience for a user.")]
    [HelpCommand("`<event>` `<user>` `<amount>` `[comment]`")]
    public async Task Modify(
        [Summary("event-name")][Choice("event-a", "EventA"), Choice("event-b", "EventB")] string eventName,
        IUser user,
        decimal amount,
        string? comment = null)
    {
        var dbUserLevel = await GetRecord(user);
        if (dbUserLevel == null)
            return;

        var before = eventName == "EventA" ? dbUserLevel.EventAExperience : dbUserLevel.EventBExperience;

        await guildExperienceService.SetEventExperience(eventName, dbUserLevel, Math.Max(0, Math.Min(decimal.MaxValue, before + amount)));

        await SendExperience(eventName, dbUserLevel, before, comment);
    }

    [SlashCommand("set", "Set event experience for a user.")]
    [HelpCommand("`<event>` `<user>` `<amount>` `[comment]`")]
    public async Task Set(
        [Summary("event-name")][Choice("event-a", "EventA"), Choice("event-b", "EventB")] string eventName,
        IUser user,
        decimal amount,
        string? comment = null)
    {
        var dbUserLevel = await GetRecord(user);
        if (dbUserLevel == null)
            return;

        var before = eventName == "EventA" ? dbUserLevel.EventAExperience : dbUserLevel.EventBExperience;

        await guildExperienceService.SetEventExperience(eventName, dbUserLevel, Math.Max(0, Math.Min(decimal.MaxValue, amount)));

        await SendExperience(eventName, dbUserLevel, before, comment);
    }

    [SlashCommand("reset", "Set event experience for all users to 0.")]
    [HelpCommand("`<event>` `<confirm>`")]
    public async Task Reset(
        [Summary("event-name")][Choice("event-a", "EventA"), Choice("event-b", "EventB")] string eventName,
        bool confirm)
    {
        if (confirm)
        {
            await guildExperienceService.ResetEventExperience(eventName, Context.Guild.Id);

            await RespondAsync($"All {eventName} experience has been reset.");
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

    private Task SendExperience(string eventName, UserLevel userLevel, decimal? before, string? comment)
    {
        var current = eventName == "EventA" ? userLevel.EventAExperience : userLevel.EventBExperience;

        var beforeText = before != null ? $" It used to be {before:N2}." : "";

        var commentText = comment != null ? $"\nComment: {comment.SanitizeMD()}" : "";

        return RespondAsync($"{userLevel.UserId.GetUserMention()} has {current:N2} {eventName} experience.{beforeText}{commentText}", allowedMentions: AllowedMentions.None);
    }
}
