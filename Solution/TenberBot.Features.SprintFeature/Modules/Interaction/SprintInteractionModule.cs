﻿using Discord;
using Discord.Interactions;
using TenberBot.Features.SprintFeature.Data.Enums;
using TenberBot.Features.SprintFeature.Data.InteractionParents;
using TenberBot.Features.SprintFeature.Data.Models;
using TenberBot.Features.SprintFeature.Data.Services;
using TenberBot.Features.SprintFeature.Data.UserStats;
using TenberBot.Features.SprintFeature.Modals.Sprint;
using TenberBot.Shared.Features.Data.Ids;
using TenberBot.Shared.Features.Data.POCO;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;

namespace TenberBot.Features.SprintFeature.Modules.Interaction;

[EnabledInDm(false)]
public class SprintInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ISprintDataService sprintDataService;
    private readonly IInteractionParentDataService interactionParentDataService;
    private readonly IUserStatDataService userStatDataService;

    public SprintInteractionModule(
        ISprintDataService sprintDataService,
        IInteractionParentDataService interactionParentDataService,
        IUserStatDataService userStatDataService)
    {
        this.sprintDataService = sprintDataService;
        this.interactionParentDataService = interactionParentDataService;
        this.userStatDataService = userStatDataService;
    }

    [ComponentInteraction("sprint:join,*")]
    public async Task SprintJoin(ulong messageId)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParents.Sprint, messageId);
        if (parent == null)
            return;

        var sprint = await sprintDataService.GetById(parent.GetReference<int>());
        if (sprint == null)
            return;

        if (Context.User.Id != sprint.UserId && sprint.Users.All(x => x.UserId != Context.User.Id))
            await Context.Interaction.RespondWithModalAsync<SprintJoinModal>($"sprint:join,{messageId}");
        else
            await RespondAsync("You are already a member of this sprint.", ephemeral: true);
    }

    [ModalInteraction("sprint:join,*")]
    public async Task SprintJoinModalResponse(ulong messageId, SprintJoinModal modal)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParents.Sprint, messageId);
        if (parent == null)
            return;

        var sprint = await sprintDataService.GetById(parent.GetReference<int>());
        if (sprint == null)
            return;

        sprint.Users.Add(new UserSprint { SprintId = sprint.SprintId, UserId = Context.User.Id, JoinDate = DateTime.Now, Message = modal.Message });

        await sprintDataService.Update(sprint, null!);

        await userStatDataService.Update(new UserStatMod(new GuildUserIds(Context), UserStats.Joined));

        await RespondAsync($"{Context.User.GetDisplayNameSanitized()} has joined the sprint!");

        await Context.Channel.GetAndModify(parent.MessageId, x => x.Embed = sprint.GetAsEmbed());

        _ = Task.Delay(TimeSpan.FromSeconds(5)).ContinueWith(_ => Context.Interaction.DeleteOriginalResponseAsync());
    }

    [ComponentInteraction("sprint:stop,*")]
    public async Task SprintStop(ulong messageId)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParents.Sprint, messageId);
        if (parent == null)
            return;

        var sprint = await sprintDataService.GetById(parent.GetReference<int>());
        if (sprint == null)
            return;

        if (sprint.Users.Any(x => x.UserId == Context.User.Id))
            await Context.Interaction.RespondWithModalAsync<SprintStopModal>($"sprint:stop,{messageId}");
        else
            await RespondAsync("You are not a member of this sprint.", ephemeral: true);
    }

    [ModalInteraction("sprint:stop,*")]
    public async Task SprintStopModalResponse(ulong messageId, SprintStopModal modal)
    {
        if (modal.Text != "stop")
            return;

        var parent = await interactionParentDataService.GetByMessageId(InteractionParents.Sprint, messageId);
        if (parent == null)
            return;

        var sprint = await sprintDataService.GetById(parent.GetReference<int>());
        if (sprint == null)
            return;

        var userSprint = sprint.Users.FirstOrDefault(x => x.UserId == Context.User.Id);
        if (userSprint == null)
            return;

        if (sprint.UserId == Context.User.Id)
        {
            await sprintDataService.Update(sprint, new Sprint { SprintStatus = SprintStatus.Stopped, });

            await RespondAsync($"Hey, {sprint.UserMentions}, the sprint has been stopped early.");

            await Context.Channel.GetAndModify(parent.MessageId, x =>
            {
                x.Content = null;
                x.Embed = sprint.GetAsEmbed();
                x.Components = new ComponentBuilder().Build();
            });

            await interactionParentDataService.Delete(parent);
        }
        else
        {
            sprint.Users.Remove(userSprint);

            await sprintDataService.Update(sprint, null!);

            await RespondAsync($"{Context.User.GetDisplayNameSanitized()} has left the sprint early.");

            await Context.Channel.GetAndModify(parent.MessageId, x => x.Embed = sprint.GetAsEmbed());
        }

        _ = Task.Delay(TimeSpan.FromSeconds(5)).ContinueWith(_ => Context.Interaction.DeleteOriginalResponseAsync());
    }
}
