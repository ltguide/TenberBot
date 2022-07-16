using Discord;
using Discord.Commands;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Data.Services;
using TenberBot.Results.Command;
using TenberBot.Services;

namespace TenberBot.Modules.Command;

public class TimerCommandModule : ModuleBase<SocketCommandContext>
{
    private const int MaxDuration = 86400 * 7;

    private readonly UserTimerService userTimerService;
    private readonly IUserTimerDataService userTimerDataService;
    private readonly IInteractionParentDataService interactionParentDataService;
    private readonly IUserStatDataService userStatDataService;
    private readonly CacheService cacheService;
    private readonly ILogger<TimerCommandModule> logger;

    public TimerCommandModule(
        UserTimerService userTimerService,
        IUserTimerDataService userTimerDataService,
        IInteractionParentDataService interactionParentDataService,
        IUserStatDataService userStatDataService,
        CacheService cacheService,
        ILogger<TimerCommandModule> logger)
    {
        this.userTimerService = userTimerService;
        this.userTimerDataService = userTimerDataService;
        this.interactionParentDataService = interactionParentDataService;
        this.userStatDataService = userStatDataService;
        this.cacheService = cacheService;
        this.logger = logger;
    }

    [Command("timer")]
    public Task<RuntimeResult> TimerNoParams()
    {
        return Task.FromResult<RuntimeResult>(DeleteResult.FromError("Please provide a duration for the timer, e.g. `30m` or `1.5h`. You can include a message as well if you'd like."));
    }

    [Command("timer")]
    [Summary("Set a timer to get a reminder.\nDurations should be in a short form, e.g. `30m` or `1.5h`")]
    [Remarks("`<duration>` `[message]`")]
    public async Task<RuntimeResult> Timer(TimeSpan duration, [Remainder] string? message = null)
    {
        if (duration.TotalSeconds < 10 || duration.TotalSeconds > MaxDuration)
            return DeleteResult.FromError("Sorry, the duration of a timer must be at least **10 seconds** and no more than a **week**.");

        var userTimer = new UserTimer
        {
            ChannelId = Context.Channel.Id,
            UserId = Context.User.Id,
            StartDate = DateTime.Now,
            FinishDate = DateTime.Now.AddSeconds(duration.TotalSeconds),
            Duration = Program.BaseDuration.AddSeconds(Math.Min(MaxDuration - 1, duration.TotalSeconds)),
            Detail = message,
        };

        await userTimerDataService.Add(userTimer);

        (await userStatDataService.GetOrAddByContext(Context)).TimersCreated++;

        await userStatDataService.Save();

        await SendEmbed(userTimer);

        userTimerService.Cycle();

        return DeleteResult.FromSuccess();
    }

    private async Task SendEmbed(UserTimer userTimer)
    {
        var reply = await Context.Message.ReplyAsync($"I've set a timer for you! It'll go off {TimestampTag.FromDateTime(userTimer.FinishDate.ToUniversalTime(), TimestampTagStyles.LongDateTime)}.");

        var parent = await interactionParentDataService.Set(new InteractionParent
        {
            GuildId = Context.Guild.Id,
            ChannelId = userTimer.ChannelId,
            UserId = userTimer.UserId,
            InteractionParentType = InteractionParentType.UserTimer,
            MessageId = reply.Id,
        }
        .SetReference(userTimer.UserTimerId));

        var components = new ComponentBuilder()
            .WithButton("Cancel", $"user-timer:stop,{reply.Id}", ButtonStyle.Secondary, new Emoji("❌"));

        await reply.ModifyAsync(x => x.Components = components.Build());
    }
}
