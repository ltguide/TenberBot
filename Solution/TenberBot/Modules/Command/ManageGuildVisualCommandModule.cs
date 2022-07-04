using Discord;
using Discord.Commands;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Data.Services;
using TenberBot.Extensions;
using TenberBot.Results.Command;
using TenberBot.Services;

namespace TenberBot.Modules.Command;

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
    [Summary("Manage random visuals.")]
    public Task<RuntimeResult> NoSubCommand()
    {
        return Task.FromResult<RuntimeResult>(CustomResult.FromError("Provide an option of: Add, Delete"));
    }

    [Command("add")]
    public async Task<RuntimeResult> Add(VisualType? visualType = null, [Remainder] string? url = null)
    {
        if (visualType == null)
            return CustomResult.FromError($"Provide an option of: {string.Join(", ", Enum.GetNames<VisualType>())}");

        url = Context.Message.Attachments.FirstOrDefault()?.Url ?? Context.Message.Embeds.FirstOrDefault()?.Url ?? url;

        if (url == null)
            return CustomResult.FromError($"I couldn't locate a file in your message.");

        var file = await webService.GetFileAttachment(url);
        if (file == null)
            return CustomResult.FromError($"I failed to download the file. Is it an image? 😦");

        var visual = new Visual(file.Value) { VisualType = visualType.Value, Url = url };

        await visualDataService.Add(visual);

        await Context.Channel.SendFileAsync(
            visual.AsAttachment(),
            $"Added {visualType} visual #{visual.VisualId} - {visual.Filename.SanitizeMD()}",
            messageReference: Context.Message.GetReferenceTo()
        );

        return CustomResult.FromSuccess();
    }

    [Command("delete", ignoreExtraArgs: true)]
    public async Task<RuntimeResult> Delete(VisualType? visualType = null, int? id = null)
    {
        if (visualType == null)
            return CustomResult.FromError($"Provide an option of: {string.Join(", ", Enum.GetNames<VisualType>())}");

        if (id == null)
            return CustomResult.FromError($"An ID is required.");

        var visual = await visualDataService.GetById(visualType.Value, id.Value);
        if (visual == null)
            return CustomResult.FromError($"I couldn't find {visualType} visual #{id}.");

        await visualDataService.Delete(visual);

        await Context.Message.ReplyAsync($"Deleted {visualType} visual #{visual.VisualId} - {visual.Filename.SanitizeMD()}");

        return CustomResult.FromSuccess();
    }
}
