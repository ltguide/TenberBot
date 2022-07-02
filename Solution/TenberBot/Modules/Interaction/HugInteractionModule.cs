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

    public HugInteractionModule(
        IHugDataService hugDataService)
    {
        this.hugDataService = hugDataService;
    }

    [ComponentInteraction("hug:add,*,*")]
    public async Task HugAdd(HugType hugType, ulong messageId)
    {
        await Context.Interaction.RespondWithModalAsync<HugAddModal>($"hug:add,{hugType},{messageId}", modifyModal: (builder) => builder.Title += hugType);
    }

    [ModalInteraction("hug:add,*,*")]
    public async Task HugAddModalResponse(HugType hugType, ulong messageId, HugAddModal modal)
    {
        var hug = new Hug { HugType = hugType, Text = modal.Text };

        await hugDataService.Add(hug);

        await RespondAsync($"Added {hugType} hug #{hug.HugId} - {hug.Text.SanitizeMD()}");

        await UpdateOriginalMessage(hugType, messageId);
    }

    [ComponentInteraction("hug:delete,*,*")]
    public async Task HugDelete(HugType hugType, ulong messageId)
    {
        await Context.Interaction.RespondWithModalAsync<HugDeleteModal>($"hug:delete,{hugType},{messageId}", modifyModal: (builder) => builder.Title += hugType);
    }

    [ModalInteraction("hug:delete,*,*")]
    public async Task HugDeleteModalResponse(HugType hugType, ulong messageId, HugDeleteModal modal)
    {
        var hug = await hugDataService.GetById(hugType, modal.Text);
        if (hug == null)
        {
            await RespondAsync($"I couldn't find {hugType} hug #{modal.Text}.", ephemeral: true);
            return;
        }

        await hugDataService.Delete(hug);

        await RespondAsync($"Deleted {hugType} hug #{hug.HugId} - {hug.Text.SanitizeMD()}");

        await UpdateOriginalMessage(hugType, messageId);
    }

    private async Task UpdateOriginalMessage(HugType hugType, ulong messageId)
    {
        await Context.Channel.ModifyEmbed(messageId, await hugDataService.GetAllAsEmbed(hugType));
    }
}
