using Discord;
using Discord.Interactions;
using TenberBot.Features.FortuneFeature.Data.Models;
using TenberBot.Features.FortuneFeature.Data.Services;
using TenberBot.Features.FortuneFeature.Modals.Fortune;
using TenberBot.Shared.Features.Data.Enums;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;
using TenberBot.Shared.Features.Extensions.Strings;

namespace TenberBot.Features.FortuneFeature.Modules.Interaction;

[RequireUserPermission(GuildPermission.ManageGuild)]
[EnabledInDm(false)]
public class ManageFortuneInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IFortuneDataService fortuneDataService;
    private readonly IInteractionParentDataService interactionParentDataService;

    public ManageFortuneInteractionModule(
        IFortuneDataService fortuneDataService,
        IInteractionParentDataService interactionParentDataService)
    {
        this.fortuneDataService = fortuneDataService;
        this.interactionParentDataService = interactionParentDataService;
    }

    [ComponentInteraction("fortune:add,*")]
    public async Task FortuneAdd(ulong messageId)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.Fortune, messageId);
        if (parent == null)
            return;

        await Context.Interaction.RespondWithModalAsync<FortuneAddModal>($"fortune:add,{messageId}");
    }

    [ModalInteraction("fortune:add,*")]
    public async Task FortuneAddModalResponse(ulong messageId, FortuneAddModal modal)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.Fortune, messageId);
        if (parent == null)
            return;

        var fortune = new Fortune { Text = modal.Text };

        await fortuneDataService.Add(fortune);

        await RespondAsync($"{Context.User.Mention} added fortune #{fortune.FortuneId} - {fortune.Text.SanitizeMD()}", allowedMentions: AllowedMentions.None);

        await UpdateOriginalMessage(messageId);
    }

    [ComponentInteraction("fortune:delete,*")]
    public async Task FortuneDelete(ulong messageId)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.Fortune, messageId);
        if (parent == null)
            return;

        await Context.Interaction.RespondWithModalAsync<FortuneDeleteModal>($"fortune:delete,{messageId}");
    }

    [ModalInteraction("fortune:delete,*")]
    public async Task FortuneDeleteModalResponse(ulong messageId, FortuneDeleteModal modal)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.Fortune, messageId);
        if (parent == null)
            return;

        var fortune = await fortuneDataService.GetById(modal.Text);
        if (fortune == null)
        {
            await RespondAsync($"I couldn't find fortune #{modal.Text}.", ephemeral: true);
            return;
        }

        await fortuneDataService.Delete(fortune);

        await RespondAsync($"{Context.User.Mention} deleted fortune #{fortune.FortuneId} - {fortune.Text.SanitizeMD()}", allowedMentions: AllowedMentions.None);

        await UpdateOriginalMessage(messageId);
    }

    private async Task UpdateOriginalMessage(ulong messageId)
    {
        var embed = await fortuneDataService.GetAllAsEmbed();

        await Context.Channel.GetAndModify(messageId, x => x.Embed = embed);
    }
}
