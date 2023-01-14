using Discord;
using Discord.Commands;
using TenberBot.Features.HugFeature.Data.Enums;
using TenberBot.Features.HugFeature.Data.InteractionParents;
using TenberBot.Features.HugFeature.Data.Services;
using TenberBot.Shared.Features.Data.Models;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;
using TenberBot.Shared.Features.Results.Command;

namespace TenberBot.Features.HugFeature.Modules.Command;

[Remarks("Server Management")]
[RequireUserPermission(GuildPermission.ManageGuild)]
public class ManageGuildCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly IHugDataService hugDataService;
    private readonly IInteractionParentDataService interactionParentDataService;

    public ManageGuildCommandModule(
        IHugDataService hugDataService,
        IInteractionParentDataService interactionParentDataService)
    {
        this.hugDataService = hugDataService;
        this.interactionParentDataService = interactionParentDataService;
    }

    [Command("hugs", ignoreExtraArgs: true)]
    [Summary("Manage random hugs.")]
    [Remarks("`<HugType>`")]
    public async Task<RuntimeResult> HugsList(HugType? hugType = null)
    {
        if (hugType == null)
            return RemainResult.FromError($"Provide an option of: {string.Join(", ", Enum.GetNames<HugType>())}");

        var reply = await Context.Message.ReplyAsync(embed: await hugDataService.GetAllAsEmbed(hugType.Value));

        await SetParent(InteractionParents.Embed, reply.Id, hugType);

        var components = new ComponentBuilder()
            .WithButton("Add", $"hug:add,{reply.Id}", ButtonStyle.Success, new Emoji("➕"))
            .WithButton("Delete", $"hug:delete,{reply.Id}", ButtonStyle.Danger, new Emoji("🗑"));

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
