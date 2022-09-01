using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using TenberBot.Features.UserTimerFeature.Data.Enums;
using TenberBot.Features.UserTimerFeature.Data.Models;
using TenberBot.Features.UserTimerFeature.Data.Services;
using TenberBot.Shared.Features.Data.Enums;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;
using TenberBot.Shared.Features.Extensions.Mentions;

namespace TenberBot.Features.UserTimerFeature.Services;

public class UserTimerService : DiscordClientService
{
    private TaskCompletionSource taskCompletionSource = null!;

    private readonly IInteractionParentDataService interactionParentDataService;
    private readonly IUserTimerDataService userTimerDataService;

    public UserTimerService(
        IInteractionParentDataService interactionParentDataService,
        IUserTimerDataService userTimerDataService,
        DiscordSocketClient client,
        ILogger<DiscordClientService> logger) : base(client, logger)
    {
        this.interactionParentDataService = interactionParentDataService;
        this.userTimerDataService = userTimerDataService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Client.WaitForReadyAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var userTimers = await userTimerDataService.GetAllActive();

                foreach (var userTimer in userTimers)
                {
                    if (userTimer.GetNextStatus() is not UserTimerStatus status)
                        continue;

                    await userTimerDataService.Update(userTimer, new UserTimer { UserTimerStatus = status, });

                    var parent = await interactionParentDataService.GetByReference(InteractionParentType.UserTimer, userTimer.UserTimerId.ToString());

                    var channel = await Client.GetChannelAsync(userTimer.ChannelId) as SocketTextChannel;

                    if (channel != null)
                    {
                        var reference = parent == null ? null : new MessageReference(parent.MessageId);

                        var detail = userTimer.Detail != null ? $"\n\nYou included the message: {userTimer.Detail}" : "";

                        await channel.SendMessageAsync($"Hey, {userTimer.UserId.GetUserMention()}, your timer has run out.{detail}", messageReference: reference);
                    }


                    if (parent == null)
                        continue;

                    if (channel != null)
                        await channel.GetAndModify(parent.MessageId, x => x.Components = new ComponentBuilder().Build());

                    await interactionParentDataService.Delete(parent);
                }

                taskCompletionSource = new();

                try
                {
                    var delay = GetDelay(userTimers);

                    if (delay != 0)
                    {
                        Logger.LogInformation($"UserTimerService next delay: {delay}");

                        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(delay), stoppingToken).ConfigureAwait(false);
                    }
                    else
                        await taskCompletionSource.Task.WaitAsync(stoppingToken).ConfigureAwait(false);
                }
                catch (TimeoutException)
                { }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "oops");
            }
        }
    }

    public void Cycle()
    {
        taskCompletionSource?.TrySetResult();
    }

    private static int GetDelay(IList<UserTimer> userTimers)
    {
        var finishDate = userTimers.Where(x => x.UserTimerStatus == UserTimerStatus.Started).OrderBy(x => x.FinishDate).FirstOrDefault()?.FinishDate;
        if (finishDate == null)
            return 0;

        return (int)Math.Ceiling(finishDate.Value.Subtract(DateTime.Now).TotalSeconds) + 1;
    }
}