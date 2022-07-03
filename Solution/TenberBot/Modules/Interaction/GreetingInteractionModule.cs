using Discord;
using Discord.Interactions;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Data.Services;
using TenberBot.Extensions;
using TenberBot.Modals.Greeting;

namespace TenberBot.Modules.Interaction;

[RequireUserPermission(GuildPermission.ManageGuild)]
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
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.Greeting, messageId);
        if (parent == null)
            return;

        await Context.Interaction.RespondWithModalAsync<GreetingAddModal>($"greeting:add,{messageId}", modifyModal: (builder) => builder.Title += parent.Reference);
    }

    [ModalInteraction("greeting:add,*")]
    public async Task GreetingAddModalResponse(ulong messageId, GreetingAddModal modal)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.Greeting, messageId);
        if (parent == null)
            return;

        var reference = (GreetingType)parent.Reference!;

        var greeting = new Greeting { GreetingType = reference, Text = modal.Text };

        await greetingDataService.Add(greeting);

        await RespondAsync($"{Context.User.Mention} added {reference} greeting #{greeting.GreetingId} - {greeting.Text.SanitizeMD()}", allowedMentions: AllowedMentions.None);

        await UpdateOriginalMessage(reference, messageId);
    }

    [ComponentInteraction("greeting:delete,*")]
    public async Task GreetingDelete(ulong messageId)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.Greeting, messageId);
        if (parent == null)
            return;

        await Context.Interaction.RespondWithModalAsync<GreetingDeleteModal>($"greeting:delete,{messageId}", modifyModal: (builder) => builder.Title += parent.Reference);
    }

    [ModalInteraction("greeting:delete,*")]
    public async Task GreetingDeleteModalResponse(ulong messageId, GreetingDeleteModal modal)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.Greeting, messageId);
        if (parent == null)
            return;

        var reference = (GreetingType)parent.Reference!;

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
        await Context.Channel.GetAndModify(messageId, async (x) => x.Embed = await greetingDataService.GetAllAsEmbed(greetingType));
    }
}
