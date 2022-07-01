using Discord;
using Discord.Interactions;
using TenberBot.Data.Models;
using TenberBot.Data.Services;
using TenberBot.Extensions;
using TenberBot.Modals.BotStatus;

namespace TenberBot.Modules.Interaction;

[RequireUserPermission(GuildPermission.ManageGuild)]
public class BotStatusInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IBotStatusDataService botStatusDataService;

    public BotStatusInteractionModule(
        IBotStatusDataService botStatusDataService)
    {
        this.botStatusDataService = botStatusDataService;
    }

    [ComponentInteraction("botstatus:add,*")]
    public async Task BotStatusAdd(ulong messageId)
    {
        await Context.Interaction.RespondWithModalAsync<BotStatusAddModal>($"botstatus:add,{messageId}");
    }

    [ModalInteraction("botstatus:add,*")]
    public async Task BotStatusAddModalResponse(ulong messageId, BotStatusAddModal modal)
    {
        var botStatus = new BotStatus { Text = modal.Text.SanitizeMD() };

        await botStatusDataService.Add(botStatus);

        await RespondAsync($"Added bot status #{botStatus.BotStatusId} - {botStatus.Text}");

        await UpdateOriginalMessage(messageId);
    }

    [ComponentInteraction("botstatus:delete,*")]
    public async Task BotStatusDelete(ulong messageId)
    {
        await Context.Interaction.RespondWithModalAsync<BotStatusDeleteModal>($"botstatus:delete,{messageId}");
    }

    [ModalInteraction("botstatus:delete,*")]
    public async Task BotStatusDeleteModalResponse(ulong messageId, BotStatusDeleteModal modal)
    {
        var botStatus = await botStatusDataService.GetById(modal.Text);
        if (botStatus == null)
        {
            await RespondAsync($"I couldn't find bot status #{modal.Text}.", ephemeral: true);
            return;
        }

        await botStatusDataService.Delete(botStatus);

        await RespondAsync($"Deleted bot status #{botStatus.BotStatusId} - {botStatus.Text}");

        await UpdateOriginalMessage(messageId);
    }

    private async Task UpdateOriginalMessage(ulong messageId)
    {
        await Context.Channel.ModifyEmbed(messageId, await botStatusDataService.GetAllAsEmbed());
    }
}
