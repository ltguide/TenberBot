using Discord;
using Discord.Interactions;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Data.Services;
using TenberBot.Extensions;
using TenberBot.Modals.Hug;

namespace TenberBot.Modules.Interaction;

[RequireUserPermission(GuildPermission.ManageGuild)]
public class HugInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IHugDataService hugDataService;
    private readonly IInteractionParentDataService interactionParentDataService;

    public HugInteractionModule(
        IHugDataService hugDataService,
        IInteractionParentDataService interactionParentDataService)
    {
        this.hugDataService = hugDataService;
        this.interactionParentDataService = interactionParentDataService;
    }

    [ComponentInteraction("hug:add,*")]
    public async Task HugAdd(ulong messageId)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.Hug, messageId);
        if (parent == null)
            return;

        await Context.Interaction.RespondWithModalAsync<HugAddModal>($"hug:add,{messageId}", modifyModal: (builder) => builder.Title += (HugType)parent.Reference!);
    }

    [ModalInteraction("hug:add,*")]
    public async Task HugAddModalResponse(ulong messageId, HugAddModal modal)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.Hug, messageId);
        if (parent == null)
            return;

        var reference = (HugType)parent.Reference!;

        var hug = new Hug { HugType = reference, Text = modal.Text };

        await hugDataService.Add(hug);

        await RespondAsync($"{Context.User.Mention} added {reference} hug #{hug.HugId} - {hug.Text.SanitizeMD()}", allowedMentions: AllowedMentions.None);

        await UpdateOriginalMessage(reference, messageId);
    }

    [ComponentInteraction("hug:delete,*")]
    public async Task HugDelete(ulong messageId)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.Hug, messageId);
        if (parent == null)
            return;

        await Context.Interaction.RespondWithModalAsync<HugDeleteModal>($"hug:delete,{messageId}", modifyModal: (builder) => builder.Title += (HugType)parent.Reference!);
    }

    [ModalInteraction("hug:delete,*")]
    public async Task HugDeleteModalResponse(ulong messageId, HugDeleteModal modal)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.Hug, messageId);
        if (parent == null)
            return;

        var reference = (HugType)parent.Reference!;

        var hug = await hugDataService.GetById(reference, modal.Text);
        if (hug == null)
        {
            await RespondAsync($"I couldn't find {reference} hug #{modal.Text}.", ephemeral: true);
            return;
        }

        await hugDataService.Delete(hug);

        await RespondAsync($"{Context.User.Mention} deleted {reference} hug #{hug.HugId} - {hug.Text.SanitizeMD()}", allowedMentions: AllowedMentions.None);

        await UpdateOriginalMessage(reference, messageId);
    }

    private async Task UpdateOriginalMessage(HugType hugType, ulong messageId)
    {
        var embed = await hugDataService.GetAllAsEmbed(hugType);

        await Context.Channel.GetAndModify(messageId, (x) => x.Embed = embed);
    }
}
