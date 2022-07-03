using Discord;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Discord.WebSocket;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Data.Services;
using TenberBot.Extensions;

namespace TenberBot.Services;

public class SprintService : DiscordClientService
{
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
            var sprints = await sprintDataService.GetAllActive();

            foreach (var sprint in sprints)
            {
                if (sprint.GetNextStatus() is not SprintStatus status)
                    continue;

                await sprintDataService.Update(sprint, new Sprint { SprintStatus = status, });

                if (await Client.GetChannelAsync(sprint.ChannelId) is not SocketTextChannel channel)
                    continue;


                var parent = await interactionParentDataService.GetById(InteractionParentType.Sprint, sprint.ChannelId, sprint.UserId);

                var reference = parent == null ? null : new MessageReference(parent.MessageId);

                if (status == SprintStatus.Finished)
                    await channel.SendMessageAsync($"***That's a wrap!***\n\nHey, {sprint.UserMentions}, how'd ya'll do? ", messageReference: reference);
                else
                {
                    var reply = await channel.SendMessageAsync($"**Here we go!** Your sprint is starting.\n\nHey, {sprint.UserMentions}, do your best! ", messageReference: reference);
                    reply.DeleteSoon(TimeSpan.FromSeconds(15));
                }


                if (parent == null)
                    continue;

                if (sprint.SprintStatus == SprintStatus.Finished)
                {
                    await channel.GetAndModify(parent.MessageId, (x) =>
                    {
                        x.Embed = sprint.GetAsEmbed();
                        x.Components = new ComponentBuilder().Build();
                    });

                    await interactionParentDataService.Delete(parent);
                }
                else
                    await channel.GetAndModify(parent.MessageId, (x) =>
                    {
                        x.Embed = sprint.GetAsEmbed();
                        x.Content = null;
                    });
            }

            await Task.Delay(8 * 1000, stoppingToken);
        }
    }
}