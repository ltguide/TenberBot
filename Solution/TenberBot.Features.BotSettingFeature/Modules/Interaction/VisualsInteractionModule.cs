using Discord;
using Discord.Interactions;
using TenberBot.Features.BotSettingFeature.Handlers;
using TenberBot.Shared.Features.Data.Models;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.Strings;
using TenberBot.Shared.Features.Results.Interaction;
using TenberBot.Shared.Features.Services;

namespace TenberBot.Features.CommandPrefixFeature.Modules.Interaction;

[Group("visuals", "Manage visuals for the bot.")]
[DefaultMemberPermissions(GuildPermission.ManageGuild)]
public class VisualsInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IVisualDataService visualDataService;
    private readonly WebService webService;

    public VisualsInteractionModule(
        IVisualDataService visualDataService,
        WebService webService)
    {
        this.visualDataService = visualDataService;
        this.webService = webService;
    }

    [SlashCommand("add", "Add a visual.")]
    public async Task<RuntimeResult> Add(
        [Summary("visual-type"), Autocomplete(typeof(VisualTypeAutocompleteHandler))] string visualType,
        string? url = null,
        IAttachment? image = null)
    {
        if (image != null)
            url = image.Url;

        if (url == null)
            return EphemeralResult.FromError($"I couldn't locate a file in your message.");

        var file = await webService.GetFileAttachment(url);
        if (file == null)
            return EphemeralResult.FromError($"I failed to download the file. Is it an image? 😦");

        var visual = new Visual(file.Value) { VisualType = visualType, Url = url };

        await visualDataService.Add(visual);

        await RespondWithFileAsync(visual.AsAttachment(), $"Added `{visualType}` visual #{visual.VisualId} - {visual.Filename.SanitizeMD()}");

        return EphemeralResult.FromSuccess();
    }

    [SlashCommand("delete", "Delete a visual.")]
    public async Task<RuntimeResult> Add(
        [Summary("visual-type"), Autocomplete(typeof(VisualTypeAutocompleteHandler))] string visualType,
        int id)
    {
        var visual = await visualDataService.GetById(visualType, id);
        if (visual == null)
            return EphemeralResult.FromError($"I couldn't find `{visualType}` visual #{id}.");

        await visualDataService.Delete(visual);

        await RespondAsync($"Deleted `{visualType}` visual #{visual.VisualId} - {visual.Filename.SanitizeMD()}");

        return EphemeralResult.FromSuccess();
    }
}
