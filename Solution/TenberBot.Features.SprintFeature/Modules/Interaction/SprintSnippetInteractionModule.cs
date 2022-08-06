using Discord;
using Discord.Interactions;
using TenberBot.Features.SprintFeature.Data.Enums;
using TenberBot.Features.SprintFeature.Data.Models;
using TenberBot.Features.SprintFeature.Data.Services;
using TenberBot.Features.SprintFeature.Modals.SprintSnippet;
using TenberBot.Shared.Features.Data.Enums;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;
using TenberBot.Shared.Features.Extensions.Strings;

namespace TenberBot.Features.SprintFeature.Modules.Interaction;

[RequireUserPermission(GuildPermission.ManageGuild)]
public class SprintSnippetInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ISprintSnippetDataService sprintSnippetDataService;
    private readonly IInteractionParentDataService interactionParentDataService;

    public SprintSnippetInteractionModule(
        ISprintSnippetDataService sprintSnippetDataService,
        IInteractionParentDataService interactionParentDataService)
    {
        this.sprintSnippetDataService = sprintSnippetDataService;
        this.interactionParentDataService = interactionParentDataService;
    }

    [ComponentInteraction("sprint-snippet:add,*")]
    public async Task SprintSnippetAdd(ulong messageId)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.SprintSnippet, messageId);
        if (parent == null)
            return;

        await Context.Interaction.RespondWithModalAsync<SprintSnippetAddModal>($"sprint-snippet:add,{messageId}", modifyModal: (builder) => builder.Title += parent.GetReference<SprintSnippetType>());
    }

    [ModalInteraction("sprint-snippet:add,*")]
    public async Task SprintSnippetAddModalResponse(ulong messageId, SprintSnippetAddModal modal)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.SprintSnippet, messageId);
        if (parent == null)
            return;

        var reference = parent.GetReference<SprintSnippetType>();

        var sprintSnippet = new SprintSnippet { SprintSnippetType = reference, Text = modal.Text };

        await sprintSnippetDataService.Add(sprintSnippet);

        await RespondAsync($"{Context.User.Mention} added {reference} sprint snippet #{sprintSnippet.SprintSnippetId} - {sprintSnippet.Text.SanitizeMD()}", allowedMentions: AllowedMentions.None);

        await UpdateOriginalMessage(reference, messageId);
    }

    [ComponentInteraction("sprint-snippet:delete,*")]
    public async Task SprintSnippetDelete(ulong messageId)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.SprintSnippet, messageId);
        if (parent == null)
            return;

        await Context.Interaction.RespondWithModalAsync<SprintSnippetDeleteModal>($"sprint-snippet:delete,{messageId}", modifyModal: (builder) => builder.Title += parent.GetReference<SprintSnippetType>());
    }

    [ModalInteraction("sprint-snippet:delete,*")]
    public async Task SprintSnippetDeleteModalResponse(ulong messageId, SprintSnippetDeleteModal modal)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.SprintSnippet, messageId);
        if (parent == null)
            return;

        var reference = parent.GetReference<SprintSnippetType>();

        var sprintSnippet = await sprintSnippetDataService.GetById(reference, modal.Text);
        if (sprintSnippet == null)
        {
            await RespondAsync($"I couldn't find {reference} sprint snippet #{modal.Text}.", ephemeral: true);
            return;
        }

        await sprintSnippetDataService.Delete(sprintSnippet);

        await RespondAsync($"{Context.User.Mention} deleted {reference} sprint snippet #{sprintSnippet.SprintSnippetId} - {sprintSnippet.Text.SanitizeMD()}", allowedMentions: AllowedMentions.None);

        await UpdateOriginalMessage(reference, messageId);
    }

    private async Task UpdateOriginalMessage(SprintSnippetType sprintSnippetType, ulong messageId)
    {
        var embed = await sprintSnippetDataService.GetAllAsEmbed(sprintSnippetType);

        await Context.Channel.GetAndModify(messageId, x => x.Embed = embed);
    }
}
