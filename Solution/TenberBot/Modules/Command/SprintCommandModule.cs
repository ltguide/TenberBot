using Discord;
using Discord.Commands;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Data.Services;
using TenberBot.Data.Settings.Channel;
using TenberBot.Extensions;
using TenberBot.Results.Command;
using TenberBot.Services;

namespace TenberBot.Modules.Command;

public class SprintCommandModule : ModuleBase<SocketCommandContext>
{
    private const int MaxDuration = 86400;

    private readonly SprintService sprintService;
    private readonly ISprintSnippetDataService sprintSnippetDataService;
    private readonly ISprintDataService sprintDataService;
    private readonly IInteractionParentDataService interactionParentDataService;
    private readonly IUserStatDataService userStatDataService;
    private readonly CacheService cacheService;
    private readonly ILogger<SprintCommandModule> logger;

    public SprintCommandModule(
        SprintService sprintService,
        ISprintSnippetDataService sprintSnippetDataService,
        ISprintDataService sprintDataService,
        IInteractionParentDataService interactionParentDataService,
        IUserStatDataService userStatDataService,
        CacheService cacheService,
        ILogger<SprintCommandModule> logger)
    {
        this.sprintService = sprintService;
        this.sprintSnippetDataService = sprintSnippetDataService;
        this.sprintDataService = sprintDataService;
        this.interactionParentDataService = interactionParentDataService;
        this.userStatDataService = userStatDataService;
        this.cacheService = cacheService;
        this.logger = logger;
    }

    [Command("sprint")]
    public async Task<RuntimeResult> SprintNoParams()
    {
        var settings = cacheService.Get<SprintChannelSettings>(Context.Channel);
        if (settings.Mode == SprintMode.Disabled)
            return RemainResult.FromError("The sprint feature isn't configured for this channel.");

        var userSprint = await sprintDataService.GetUserById(Context.User.Id, active: true);
        if (userSprint == null)
            return DeleteResult.FromError("Please provide a duration for the sprint, e.g. `30m` or `1.5h`. You can include a message as well if you'd like.");

        var sprint = userSprint.Sprint;

        if (sprint.ChannelId != Context.Channel.Id)
            return DeleteResult.FromError($"Sorry, I can't refresh your sprint status card. Your sprint is in {sprint.ChannelId.GetChannelMention()}.");

        await SendEmbed(sprint, null);

        return DeleteResult.FromSuccess();
    }

    [Command("sprint")]
    [Summary("Sprint to Get Stuff Done™\nDurations should be in a short form, e.g. `30m` or `1.5h`")]
    [Remarks("`<duration>` `[message]`")]
    public async Task<RuntimeResult> Sprint(TimeSpan duration, [Remainder] string? message = null)
    {
        var settings = cacheService.Get<SprintChannelSettings>(Context.Channel);
        if (settings.Mode == SprintMode.Disabled)
            return RemainResult.FromError("The sprint feature isn't configured for this channel.");

        var userSprint = await sprintDataService.GetUserById(Context.User.Id, active: true);
        if (userSprint != null)
            return DeleteResult.FromError($"You are already a member of a sprint that will finish in {TimestampTag.FromDateTime(userSprint.Sprint.FinishDate.ToUniversalTime(), TimestampTagStyles.Relative)}");

        if (duration.TotalSeconds < 60 || duration.TotalSeconds > MaxDuration)
            return DeleteResult.FromError("Sorry, the duration of a sprint must be at least a **minute** and no more than a **day**.");

        var sprint = new Sprint
        {
            ChannelId = Context.Channel.Id,
            UserId = Context.User.Id,
            SprintMode = settings.Mode,
            StartDate = DateTime.Now.AddSeconds(180),
            FinishDate = DateTime.Now.AddSeconds(180 + duration.TotalSeconds),
            Duration = Program.BaseDuration.AddSeconds(Math.Min(MaxDuration - 1, duration.TotalSeconds)),
            Users = { new UserSprint { UserId = Context.User.Id, JoinDate = DateTime.Now, Message = message } },
        };

        if (settings.Mode == SprintMode.Snippet)
        {
            var characters = await sprintSnippetDataService.GetRandom(SprintSnippetType.Characters);
            var prompt = await sprintSnippetDataService.GetRandom(SprintSnippetType.Prompt);

            if (characters == null || prompt == null)
                return DeleteResult.FromError("I wasn't able to find the random details.");

            sprint.Detail = $"> **Character(s)/Pairing**: {characters.Text}\n> **Prompt**: {prompt.Text}\n\n";
        }

        await sprintDataService.Add(sprint);

        (await userStatDataService.GetOrAddByContext(Context)).SprintsCreated++;

        await userStatDataService.Save();

        await SendEmbed(sprint, $"Get ready, {settings.Role}! There's a new sprint starting soon.");

        sprintService.Cycle();

        return DeleteResult.FromSuccess();
    }

    private async Task SendEmbed(Sprint sprint, string? message)
    {
        var reply = await ReplyAsync(message, embed: sprint.GetAsEmbed());

        var parent = await interactionParentDataService.Set(new InteractionParent
        {
            GuildId = Context.Guild.Id,
            ChannelId = sprint.ChannelId,
            UserId = sprint.UserId,
            InteractionParentType = InteractionParentType.Sprint,
            MessageId = reply.Id,
        }
        .SetReference(sprint.SprintId));

        var components = new ComponentBuilder()
            .WithButton("Join", $"sprint:join,{reply.Id}", ButtonStyle.Primary, new Emoji("🤼"))
            .WithButton("Stop", $"sprint:stop,{reply.Id}", ButtonStyle.Danger, new Emoji("✖"));

        await reply.ModifyAsync(x => x.Components = components.Build());

        try
        {
            if (parent != null)
                await Context.Channel.DeleteMessageAsync(parent.Value);
        }
        catch (Exception) { }

        Context.Message.DeleteSoon();
    }
}
