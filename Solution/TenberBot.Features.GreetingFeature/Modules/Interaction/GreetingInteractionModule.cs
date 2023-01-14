using Discord;
using Discord.Interactions;
using TenberBot.Features.GreetingFeature.Data.Enums;
using TenberBot.Features.GreetingFeature.Data.InteractionParents;
using TenberBot.Features.GreetingFeature.Data.Models;
using TenberBot.Features.GreetingFeature.Data.Services;
using TenberBot.Features.GreetingFeature.Modals.Greeting;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;
using TenberBot.Shared.Features.Extensions.Strings;

namespace TenberBot.Features.GreetingFeature.Modules.Interaction;

[RequireUserPermission(GuildPermission.ManageGuild)]
[EnabledInDm(false)]
public class GreetingInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IGreetingDataService greetingDataService;
    private readonly IInteractionParentDataService interactionParentDataService;

    public GreetingInteractionModule(
        IGreetingDataService greetingDataService,
        IInteractionParentDataService interactionParentDataService)
    {
        this.greetingDataService = greetingDataService;
        this.interactionParentDataService = interactionParentDataService;
    }

    [ComponentInteraction("greeting:add,*")]
    public async Task GreetingAdd(ulong messageId)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParents.Embed, messageId);
        if (parent == null)
            return;

        await Context.Interaction.RespondWithModalAsync<GreetingAddModal>($"greeting:add,{messageId}", modifyModal: (builder) => builder.Title += parent.GetReference<GreetingType>());
    }

    [ModalInteraction("greeting:add,*")]
    public async Task GreetingAddModalResponse(ulong messageId, GreetingAddModal modal)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParents.Embed, messageId);
        if (parent == null)
            return;

        var reference = parent.GetReference<GreetingType>();

        var greeting = new Greeting { GreetingType = reference, Text = modal.Text };

        await greetingDataService.Add(greeting);

        await RespondAsync($"{Context.User.Mention} added {reference} greeting #{greeting.GreetingId} - {greeting.Text.SanitizeMD()}", allowedMentions: AllowedMentions.None);

        await UpdateOriginalMessage(reference, messageId);
    }

    [ComponentInteraction("greeting:delete,*")]
    public async Task GreetingDelete(ulong messageId)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParents.Embed, messageId);
        if (parent == null)
            return;

        await Context.Interaction.RespondWithModalAsync<GreetingDeleteModal>($"greeting:delete,{messageId}", modifyModal: (builder) => builder.Title += parent.GetReference<GreetingType>());
    }

    [ModalInteraction("greeting:delete,*")]
    public async Task GreetingDeleteModalResponse(ulong messageId, GreetingDeleteModal modal)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParents.Embed, messageId);
        if (parent == null)
            return;

        var reference = parent.GetReference<GreetingType>();

        var greeting = await greetingDataService.GetById(reference, modal.Text);
        if (greeting == null)
        {
            await RespondAsync($"I couldn't find {reference} greeting #{modal.Text}.", ephemeral: true);
            return;
        }

        await greetingDataService.Delete(greeting);

        await RespondAsync($"{Context.User.Mention} deleted {reference} greeting #{greeting.GreetingId} - {greeting.Text.SanitizeMD()}", allowedMentions: AllowedMentions.None);

        await UpdateOriginalMessage(reference, messageId);
    }

    private async Task UpdateOriginalMessage(GreetingType greetingType, ulong messageId)
    {
        var embed = await greetingDataService.GetAllAsEmbed(greetingType);

        await Context.Channel.GetAndModify(messageId, x => x.Embed = embed);
    }
}
