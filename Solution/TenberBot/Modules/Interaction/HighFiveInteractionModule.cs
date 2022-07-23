using Discord;
using Discord.Interactions;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Data.Services;
using TenberBot.Extensions;
using TenberBot.Modals.HighFive;

namespace TenberBot.Modules.Interaction;

[RequireUserPermission(GuildPermission.ManageGuild)]
public class HighFiveInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IHighFiveDataService highFiveDataService;
    private readonly IInteractionParentDataService interactionParentDataService;

    public HighFiveInteractionModule(
        IHighFiveDataService highFiveDataService,
        IInteractionParentDataService interactionParentDataService)
    {
        this.highFiveDataService = highFiveDataService;
        this.interactionParentDataService = interactionParentDataService;
    }

    [ComponentInteraction("high-five:add,*")]
    public async Task HighFiveAdd(ulong messageId)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.HighFive, messageId);
        if (parent == null)
            return;

        await Context.Interaction.RespondWithModalAsync<HighFiveAddModal>($"high-five:add,{messageId}", modifyModal: (builder) => builder.Title += parent.GetReference<HighFiveType>());
    }

    [ModalInteraction("high-five:add,*")]
    public async Task HighFiveAddModalResponse(ulong messageId, HighFiveAddModal modal)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.HighFive, messageId);
        if (parent == null)
            return;

        var reference = parent.GetReference<HighFiveType>();

        var highFive = new HighFive { HighFiveType = reference, Text = modal.Text };

        await highFiveDataService.Add(highFive);

        await RespondAsync($"{Context.User.Mention} added {reference} high-five #{highFive.HighFiveId} - {highFive.Text.SanitizeMD()}", allowedMentions: AllowedMentions.None);

        await UpdateOriginalMessage(reference, messageId);
    }

    [ComponentInteraction("high-five:delete,*")]
    public async Task HighFiveDelete(ulong messageId)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.HighFive, messageId);
        if (parent == null)
            return;

        await Context.Interaction.RespondWithModalAsync<HighFiveDeleteModal>($"high-five:delete,{messageId}", modifyModal: (builder) => builder.Title += parent.GetReference<HighFiveType>());
    }

    [ModalInteraction("high-five:delete,*")]
    public async Task HighFiveDeleteModalResponse(ulong messageId, HighFiveDeleteModal modal)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.HighFive, messageId);
        if (parent == null)
            return;

        var reference = parent.GetReference<HighFiveType>();

        var highFive = await highFiveDataService.GetById(reference, modal.Text);
        if (highFive == null)
        {
            await RespondAsync($"I couldn't find {reference} high-five #{modal.Text}.", ephemeral: true);
            return;
        }

        await highFiveDataService.Delete(highFive);

        await RespondAsync($"{Context.User.Mention} deleted {reference} high-five #{highFive.HighFiveId} - {highFive.Text.SanitizeMD()}", allowedMentions: AllowedMentions.None);

        await UpdateOriginalMessage(reference, messageId);
    }

    private async Task UpdateOriginalMessage(HighFiveType highFiveType, ulong messageId)
    {
        var embed = await highFiveDataService.GetAllAsEmbed(highFiveType);

        await Context.Channel.GetAndModify(messageId, x => x.Embed = embed);
    }
}
