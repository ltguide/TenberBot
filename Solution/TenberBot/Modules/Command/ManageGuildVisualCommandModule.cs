using Discord;
using Discord.Commands;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Data.Services;
using TenberBot.Extensions;
using TenberBot.Results.Command;
using TenberBot.Services;

namespace TenberBot.Modules.Command;

[Remarks("Server Management")]
[Group("visuals")]
[RequireUserPermission(GuildPermission.ManageGuild)]
public class ManageGuildVisualCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly IVisualDataService visualDataService;
    private readonly WebService webService;
    private readonly ILogger<ManageGuildVisualCommandModule> logger;

    public ManageGuildVisualCommandModule(
        IVisualDataService visualDataService,
        WebService webService,
        ILogger<ManageGuildVisualCommandModule> logger)
    {
        this.visualDataService = visualDataService;
        this.webService = webService;
        this.logger = logger;
    }

    [Command("", ignoreExtraArgs: true)]
    public Task<RuntimeResult> NoSubCommand()
    {
        return Task.FromResult<RuntimeResult>(RemainResult.FromError("Provide an option of: Add, Delete"));
    }

    [Command("add")]
    [Summary("Add a random visual.")]
    [Remarks("`<VisualType>` `<url>`")]
    public async Task<RuntimeResult> Add(VisualType? visualType = null, [Remainder] string? url = null)
    {
        if (visualType == null)
            return RemainResult.FromError($"Provide an option of: {string.Join(", ", Enum.GetNames<VisualType>())}");

        url = Context.Message.Attachments.FirstOrDefault()?.Url ?? Context.Message.Embeds.FirstOrDefault()?.Url ?? url;

        if (url == null)
            return DeleteResult.FromError($"I couldn't locate a file in your message.");

        var file = await webService.GetFileAttachment(url);
        if (file == null)
            return DeleteResult.FromError($"I failed to download the file. Is it an image? 😦");

        var visual = new Visual(file.Value) { VisualType = visualType.Value, Url = url };

        await visualDataService.Add(visual);

        await Context.Channel.SendFileAsync(
            visual.AsAttachment(),
            $"Added {visualType} visual #{visual.VisualId} - {visual.Filename.SanitizeMD()}",
            messageReference: Context.Message.GetReferenceTo()
        );

        return DeleteResult.FromSuccess();
    }

    [Command("delete", ignoreExtraArgs: true)]
    [Summary("Delete a random visual.")]
    [Remarks("`<VisualType>` `<id#>`")]
    public async Task<RuntimeResult> Delete(VisualType? visualType = null, int? id = null)
    {
        if (visualType == null)
            return RemainResult.FromError($"Provide an option of: {string.Join(", ", Enum.GetNames<VisualType>())}");

        if (id == null)
            return DeleteResult.FromError($"An ID is required.");

        var visual = await visualDataService.GetById(visualType.Value, id.Value);
        if (visual == null)
            return DeleteResult.FromError($"I couldn't find {visualType} visual #{id}.");

        await visualDataService.Delete(visual);

        await Context.Message.ReplyAsync($"Deleted {visualType} visual #{visual.VisualId} - {visual.Filename.SanitizeMD()}");

        return DeleteResult.FromSuccess();
    }
}
