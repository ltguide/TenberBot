using Discord;
using Discord.Interactions;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Data.Services;
using TenberBot.Extensions;
using TenberBot.Modals.Pat;

namespace TenberBot.Modules.Interaction;

[RequireUserPermission(GuildPermission.ManageGuild)]
public class PatInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IPatDataService patDataService;
    private readonly IInteractionParentDataService interactionParentDataService;

    public PatInteractionModule(
        IPatDataService patDataService,
        IInteractionParentDataService interactionParentDataService)
    {
        this.patDataService = patDataService;
        this.interactionParentDataService = interactionParentDataService;
    }

    [ComponentInteraction("pat:add,*")]
    public async Task PatAdd(ulong messageId)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.Pat, messageId);
        if (parent == null)
            return;

        await Context.Interaction.RespondWithModalAsync<PatAddModal>($"pat:add,{messageId}", modifyModal: (builder) => builder.Title += parent.GetReference<PatType>());
    }

    [ModalInteraction("pat:add,*")]
    public async Task PatAddModalResponse(ulong messageId, PatAddModal modal)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.Pat, messageId);
        if (parent == null)
            return;

        var reference = parent.GetReference<PatType>();

        var pat = new Pat { PatType = reference, Text = modal.Text };

        await patDataService.Add(pat);

        await RespondAsync($"{Context.User.Mention} added {reference} pat #{pat.PatId} - {pat.Text.SanitizeMD()}", allowedMentions: AllowedMentions.None);

        await UpdateOriginalMessage(reference, messageId);
    }

    [ComponentInteraction("pat:delete,*")]
    public async Task PatDelete(ulong messageId)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.Pat, messageId);
        if (parent == null)
            return;

        await Context.Interaction.RespondWithModalAsync<PatDeleteModal>($"pat:delete,{messageId}", modifyModal: (builder) => builder.Title += parent.GetReference<PatType>());
    }

    [ModalInteraction("pat:delete,*")]
    public async Task PatDeleteModalResponse(ulong messageId, PatDeleteModal modal)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.Pat, messageId);
        if (parent == null)
            return;

        var reference = parent.GetReference<PatType>();

        var pat = await patDataService.GetById(reference, modal.Text);
        if (pat == null)
        {
            await RespondAsync($"I couldn't find {reference} pat #{modal.Text}.", ephemeral: true);
            return;
        }

        await patDataService.Delete(pat);

        await RespondAsync($"{Context.User.Mention} deleted {reference} pat #{pat.PatId} - {pat.Text.SanitizeMD()}", allowedMentions: AllowedMentions.None);

        await UpdateOriginalMessage(reference, messageId);
    }

    private async Task UpdateOriginalMessage(PatType patType, ulong messageId)
    {
        var embed = await patDataService.GetAllAsEmbed(patType);

        await Context.Channel.GetAndModify(messageId, x => x.Embed = embed);
    }
}
