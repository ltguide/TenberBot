using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using TenberBot.Features.MessageTimerFeature.Data.Enums;
using TenberBot.Features.MessageTimerFeature.Data.Models;
using TenberBot.Features.MessageTimerFeature.Data.Services;
using TenberBot.Shared.Features.Data.Enums;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;

namespace TenberBot.Features.MessageTimerFeature.Services;

public class MessageTimerService : DiscordClientService
{
    private TaskCompletionSource taskCompletionSource = null!;

    private readonly IInteractionParentDataService interactionParentDataService;
    private readonly IMessageTimerDataService messageTimerDataService;

    public MessageTimerService(
        IInteractionParentDataService interactionParentDataService,
        IMessageTimerDataService messageTimerDataService,
        DiscordSocketClient client,
        ILogger<DiscordClientService> logger) : base(client, logger)
    {
        this.interactionParentDataService = interactionParentDataService;
        this.messageTimerDataService = messageTimerDataService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Client.WaitForReadyAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var messageTimers = await messageTimerDataService.GetAllActive();

            foreach (var messageTimer in messageTimers)
            {
                if (messageTimer.GetNextStatus() is not MessageTimerStatus status)
                    continue;

                await messageTimerDataService.Update(messageTimer, new MessageTimer { MessageTimerStatus = status, });

                if (await Client.GetChannelAsync(messageTimer.TargetChannelId) is SocketTextChannel targetChannel)
                {
                    if (messageTimer.Data == null)
                        await targetChannel.SendMessageAsync(messageTimer.Detail);
                    else
                        await targetChannel.SendFileAsync(messageTimer.AsAttachment(), messageTimer.Detail);
                }


                var parent = await interactionParentDataService.GetByReference(InteractionParentType.MessageTimer, messageTimer.MessageTimerId.ToString());
                if (parent == null)
                    continue;

                if (await Client.GetChannelAsync(messageTimer.ChannelId) is SocketTextChannel channel)
                    await channel.GetAndModify(parent.MessageId, x => x.Components = new ComponentBuilder().Build());

                await interactionParentDataService.Delete(parent);
            }

            taskCompletionSource = new();

            try
            {
                var delay = GetDelay(messageTimers);

                if (delay != 0)
                {
                    Logger.LogInformation($"MessageTimerService next delay: {delay}");

                    await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(delay), stoppingToken).ConfigureAwait(false);
                }
                else
                    await taskCompletionSource.Task.WaitAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (TimeoutException)
            {
            }
        }
    }

    public void Cycle()
    {
        taskCompletionSource?.TrySetResult();
    }

    private static int GetDelay(IList<MessageTimer> messageTimers)
    {
        var finishDate = messageTimers.Where(x => x.MessageTimerStatus == MessageTimerStatus.Started).OrderBy(x => x.FinishDate).FirstOrDefault()?.FinishDate;
        if (finishDate == null)
            return 0;

        return (int)Math.Ceiling(finishDate.Value.Subtract(DateTime.Now).TotalSeconds) + 1;
    }
}