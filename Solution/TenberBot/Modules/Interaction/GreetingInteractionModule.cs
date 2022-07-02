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

    public GreetingInteractionModule(
        IGreetingDataService greetingDataService)
    {
        this.greetingDataService = greetingDataService;
    }

    [ComponentInteraction("greeting:add,*,*")]
    public async Task GreetingAdd(GreetingType greetingType, ulong messageId)
    {
        await Context.Interaction.RespondWithModalAsync<GreetingAddModal>($"greeting:add,{greetingType},{messageId}", modifyModal: (builder) => builder.Title += greetingType);
    }

    [ModalInteraction("greeting:add,*,*")]
    public async Task GreetingAddModalResponse(GreetingType greetingType, ulong messageId, GreetingAddModal modal)
    {
        var greeting = new Greeting { GreetingType = greetingType, Text = modal.Text };

        await greetingDataService.Add(greeting);

        await RespondAsync($"Added {greetingType} greeting #{greeting.GreetingId} - {greeting.Text.SanitizeMD()}");

        await UpdateOriginalMessage(greetingType, messageId);
    }

    [ComponentInteraction("greeting:delete,*,*")]
    public async Task GreetingDelete(GreetingType greetingType, ulong messageId)
    {
        await Context.Interaction.RespondWithModalAsync<GreetingDeleteModal>($"greeting:delete,{greetingType},{messageId}", modifyModal: (builder) => builder.Title += greetingType);
    }

    [ModalInteraction("greeting:delete,*,*")]
    public async Task GreetingDeleteModalResponse(GreetingType greetingType, ulong messageId, GreetingDeleteModal modal)
    {
        var greeting = await greetingDataService.GetById(greetingType, modal.Text);
        if (greeting == null)
        {
            await RespondAsync($"I couldn't find {greetingType} greeting #{modal.Text}.", ephemeral: true);
            return;
        }

        await greetingDataService.Delete(greeting);

        await RespondAsync($"Deleted {greetingType} greeting #{greeting.GreetingId} - {greeting.Text.SanitizeMD()}");

        await UpdateOriginalMessage(greetingType, messageId);
    }

    private async Task UpdateOriginalMessage(GreetingType greetingType, ulong messageId)
    {
        await Context.Channel.ModifyEmbed(messageId, await greetingDataService.GetAllAsEmbed(greetingType));
    }
}
