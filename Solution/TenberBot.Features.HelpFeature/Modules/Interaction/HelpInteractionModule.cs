using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using TenberBot.Features.HelpFeature.Services;

namespace TenberBot.Features.HelpFeature.Modules.Interaction;

[DefaultMemberPermissions(GuildPermission.SendMessages)]
[Discord.Interactions.RequireUserPermission(GuildPermission.SendMessages)]
public class HelpInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IHelpService helpService;

    public HelpInteractionModule(
        IHelpService helpService)
    {
        this.helpService = helpService;
    }

    [ComponentInteraction("help-page:*,*,*")]
    public async Task Page(ulong userId, string _, int currentPage)
    {
        if (Context.Interaction is not SocketMessageComponent interaction || interaction.Message is not SocketMessage message)
            return;

        if (userId != Context.User.Id)
        {
            await RespondAsync("Sorry, you can't interact with this message.", ephemeral: true);
            return;
        }

        var authorFieldInfo = typeof(SocketMessage).GetField($"<{nameof(SocketMessage.Author)}>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (authorFieldInfo == null)
            throw new InvalidOperationException("unable to find backing field; did API change?");

        authorFieldInfo.SetValue(message, Context.User);

        var messageProperties = await helpService.BuildMessage(new SocketCommandContext(Context.Client, interaction.Message), currentPage);

        if (Context.Interaction.HasResponded == false)
            await DeferAsync();

        await ModifyOriginalResponseAsync(x =>
        {
            x.Embed = messageProperties.Embed;
            x.Components = messageProperties.Components;
        });
    }
}
