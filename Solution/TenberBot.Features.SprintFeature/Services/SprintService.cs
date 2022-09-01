using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using TenberBot.Features.SprintFeature.Data.Enums;
using TenberBot.Features.SprintFeature.Data.Models;
using TenberBot.Features.SprintFeature.Data.Services;
using TenberBot.Shared.Features.Data.Enums;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.DiscordRoot;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;

namespace TenberBot.Features.SprintFeature.Services;

public class SprintService : DiscordClientService
{
    private TaskCompletionSource taskCompletionSource = null!;

    private readonly IInteractionParentDataService interactionParentDataService;
    private readonly ISprintDataService sprintDataService;

    public SprintService(
        IInteractionParentDataService interactionParentDataService,
        ISprintDataService sprintDataService,
        DiscordSocketClient client,
        ILogger<DiscordClientService> logger) : base(client, logger)
    {
        this.interactionParentDataService = interactionParentDataService;
        this.sprintDataService = sprintDataService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Client.WaitForReadyAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var sprints = await sprintDataService.GetAllActive();

                foreach (var sprint in sprints)
                {
                    if (sprint.GetNextStatus() is not SprintStatus status)
                        continue;

                    await sprintDataService.Update(sprint, new Sprint { SprintStatus = status, });

                    var parent = await interactionParentDataService.GetById(InteractionParentType.Sprint, sprint.ChannelId, sprint.UserId);

                    var channel = await Client.GetChannelAsync(sprint.ChannelId) as SocketTextChannel;

                    if (channel != null)
                    {
                        var reference = parent == null ? null : new MessageReference(parent.MessageId);

                        if (status == SprintStatus.Finished)
                            await channel.SendMessageAsync($"***That's a wrap!***\n\nHey, {sprint.UserMentions}, how'd ya'll do? ", messageReference: reference);
                        else
                        {
                            var reply = await channel.SendMessageAsync($"**Here we go!** Your sprint is starting.\n\nHey, {sprint.UserMentions}, do your best! ", messageReference: reference);
                            reply.DeleteSoon(TimeSpan.FromSeconds(15));
                        }
                    }


                    if (parent == null)
                        continue;

                    if (sprint.SprintStatus == SprintStatus.Finished)
                    {
                        if (channel != null)
                            await channel.GetAndModify(parent.MessageId, x =>
                            {
                                x.Embed = sprint.GetAsEmbed();
                                x.Components = new ComponentBuilder().Build();
                            });

                        await interactionParentDataService.Delete(parent);
                    }
                    else if (channel != null)
                        await channel.GetAndModify(parent.MessageId, x =>
                        {
                            x.Embed = sprint.GetAsEmbed();
                            x.Content = null;
                        });
                }

                taskCompletionSource = new();

                try
                {
                    var delay = GetDelay(sprints);

                    if (delay != 0)
                    {
                        Logger.LogInformation($"SprintService next delay: {delay}");

                        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(delay), stoppingToken).ConfigureAwait(false);
                    }
                    else
                        await taskCompletionSource.Task.WaitAsync(stoppingToken).ConfigureAwait(false);
                }
                catch (TimeoutException)
                { }
            }
            catch (Exception ex) when (ex is not TaskCanceledException)
            {
                Logger.LogError(ex, "oops");
            }
        }
    }

    public void Cycle()
    {
        taskCompletionSource?.TrySetResult();
    }

    private static int GetDelay(IList<Sprint> sprints)
    {
        var nextDates = new List<DateTime>();

        var startDate = sprints.Where(x => x.SprintStatus == SprintStatus.Waiting).OrderBy(x => x.StartDate).FirstOrDefault()?.StartDate;
        if (startDate != null)
            nextDates.Add(startDate.Value);

        var finishDate = sprints.Where(x => x.SprintStatus == SprintStatus.Started).OrderBy(x => x.FinishDate).FirstOrDefault()?.FinishDate;
        if (finishDate != null)
            nextDates.Add(finishDate.Value);

        if (nextDates.Count == 0)
            return 0;

        nextDates.Sort();

        return (int)Math.Ceiling(nextDates[0].Subtract(DateTime.Now).TotalSeconds) + 1;
    }
}