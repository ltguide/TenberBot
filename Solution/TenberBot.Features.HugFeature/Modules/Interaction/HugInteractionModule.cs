using Discord;
using Discord.Interactions;
using TenberBot.Features.HugFeature.Data.Enums;
using TenberBot.Features.HugFeature.Data.Models;
using TenberBot.Features.HugFeature.Data.Services;
using TenberBot.Features.HugFeature.Modals.Hug;
using TenberBot.Shared.Features.Data.Enums;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;
using TenberBot.Shared.Features.Extensions.Strings;

namespace TenberBot.Features.HugFeature.Modules.Interaction;

[RequireUserPermission(GuildPermission.ManageGuild)]
[EnabledInDm(false)]
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

        await Context.Interaction.RespondWithModalAsync<HugAddModal>($"hug:add,{messageId}", modifyModal: (builder) => builder.Title += parent.GetReference<HugType>());
    }

    [ModalInteraction("hug:add,*")]
    public async Task HugAddModalResponse(ulong messageId, HugAddModal modal)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.Hug, messageId);
        if (parent == null)
            return;

        var reference = parent.GetReference<HugType>();

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

        await Context.Interaction.RespondWithModalAsync<HugDeleteModal>($"hug:delete,{messageId}", modifyModal: (builder) => builder.Title += parent.GetReference<HugType>());
    }

    [ModalInteraction("hug:delete,*")]
    public async Task HugDeleteModalResponse(ulong messageId, HugDeleteModal modal)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.Hug, messageId);
        if (parent == null)
            return;

        var reference = parent.GetReference<HugType>();

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

        await Context.Channel.GetAndModify(messageId, x => x.Embed = embed);
    }
}
