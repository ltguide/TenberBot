using Discord;
using Discord.Commands;
using TenberBot.Features.PatFeature.Data.Enums;
using TenberBot.Features.PatFeature.Data.InteractionParents;
using TenberBot.Features.PatFeature.Data.Services;
using TenberBot.Shared.Features.Data.Models;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;
using TenberBot.Shared.Features.Results.Command;

namespace TenberBot.Features.PatFeature.Modules.Command;

[Remarks("Server Management")]
[RequireUserPermission(GuildPermission.ManageGuild)]
public class ManageGuildCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly IPatDataService patDataService;
    private readonly IInteractionParentDataService interactionParentDataService;

    public ManageGuildCommandModule(
        IPatDataService patDataService,
        IInteractionParentDataService interactionParentDataService)
    {
        this.patDataService = patDataService;
        this.interactionParentDataService = interactionParentDataService;
    }

    [Command("pats", ignoreExtraArgs: true)]
    [Summary("Manage random pats.")]
    [Remarks("`<PatType>`")]
    public async Task<RuntimeResult> PatsList(PatType? patType = null)
    {
        if (patType == null)
            return RemainResult.FromError($"Provide an option of: {string.Join(", ", Enum.GetNames<PatType>())}");

        var reply = await Context.Message.ReplyAsync(embed: await patDataService.GetAllAsEmbed(patType.Value));

        await SetParent(InteractionParents.Embed, reply.Id, patType);

        var components = new ComponentBuilder()
            .WithButton("Add", $"pat:add,{reply.Id}", ButtonStyle.Success, new Emoji("➕"))
            .WithButton("Delete", $"pat:delete,{reply.Id}", ButtonStyle.Danger, new Emoji("🗑"));

        await reply.ModifyAsync(x => x.Components = components.Build());

        return DeleteResult.FromSuccess();
    }

    private async Task SetParent(string parentType, ulong messageId, Enum? reference)
    {
        var previousParent = await interactionParentDataService.Set(new InteractionParent
        {
            GuildId = Context.Guild.Id,
            ChannelId = Context.Channel.Id,
            UserId = null,
            InteractionParentType = parentType,
            MessageId = messageId,
        }
        .SetReference(Convert.ToInt32(reference)));

        await Context.Channel.GetAndModify(previousParent, x => x.Components = new ComponentBuilder().Build());
    }
}
