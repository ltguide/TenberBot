using Discord;
using Discord.Commands;
using TenberBot.Features.HighFiveFeature.Data.Enums;
using TenberBot.Features.HighFiveFeature.Data.Services;
using TenberBot.Shared.Features.Data.Enums;
using TenberBot.Shared.Features.Data.Models;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;
using TenberBot.Shared.Features.Results.Command;

namespace TenberBot.Features.HighFiveFeature.Modules.Command;

[Remarks("Server Management")]
[RequireUserPermission(GuildPermission.ManageGuild)]
public class ManageGuildCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly IHighFiveDataService highFiveDataService;
    private readonly IInteractionParentDataService interactionParentDataService;

    public ManageGuildCommandModule(
        IHighFiveDataService highFiveDataService,
        IInteractionParentDataService interactionParentDataService)
    {
        this.highFiveDataService = highFiveDataService;
        this.interactionParentDataService = interactionParentDataService;
    }

    [Command("high-fives", ignoreExtraArgs: true)]
    [Summary("Manage random high-fives.")]
    [Remarks("`<HighFiveType>`")]
    public async Task<RuntimeResult> HighFivesList(HighFiveType? highFiveType = null)
    {
        if (highFiveType == null)
            return RemainResult.FromError($"Provide an option of: {string.Join(", ", Enum.GetNames<HighFiveType>())}");

        var reply = await Context.Message.ReplyAsync(embed: await highFiveDataService.GetAllAsEmbed(highFiveType.Value));

        await SetParent(InteractionParentType.HighFive, reply.Id, highFiveType);

        var components = new ComponentBuilder()
            .WithButton("Add", $"high-five:add,{reply.Id}", ButtonStyle.Success, new Emoji("➕"))
            .WithButton("Delete", $"high-five:delete,{reply.Id}", ButtonStyle.Danger, new Emoji("🗑"));

        await reply.ModifyAsync(x => x.Components = components.Build());

        return DeleteResult.FromSuccess();
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
