using Discord;
using Discord.Commands;
using TenberBot.Features.BotStatusFeature.Data.Services;
using TenberBot.Shared.Features.Data.Enums;
using TenberBot.Shared.Features.Data.Models;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;

namespace TenberBot.Features.BotStatusFeature.Modules.Command;

[Remarks("Server Management")]
[RequireUserPermission(GuildPermission.ManageGuild)]
public class ManageGuildCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly IBotStatusDataService botStatusDataService;
    private readonly IInteractionParentDataService interactionParentDataService;

    public ManageGuildCommandModule(
        IBotStatusDataService botStatusDataService,
        IInteractionParentDataService interactionParentDataService)
    {
        this.botStatusDataService = botStatusDataService;
        this.interactionParentDataService = interactionParentDataService;
    }

    [Command("botstatuses", ignoreExtraArgs: true)]
    [Summary("Manage random bot statuses.")]
    public async Task BotStatusesList()
    {
        var reply = await Context.Message.ReplyAsync(embed: await botStatusDataService.GetAllAsEmbed());

        await SetParent(InteractionParentType.BotStatus, reply.Id, null);

        var components = new ComponentBuilder()
            .WithButton("Add", $"bot-status:add,{reply.Id}", ButtonStyle.Success, new Emoji("➕"))
            .WithButton("Delete", $"bot-status:delete,{reply.Id}", ButtonStyle.Danger, new Emoji("🗑"));

        await reply.ModifyAsync(x => x.Components = components.Build());
    }

    private async Task SetParent(InteractionParentType parentType, ulong messageId, Enum? reference)
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
