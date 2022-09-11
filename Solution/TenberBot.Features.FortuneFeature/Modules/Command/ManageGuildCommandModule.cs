using Discord;
using Discord.Commands;
using TenberBot.Features.FortuneFeature.Data.Services;
using TenberBot.Shared.Features.Data.Enums;
using TenberBot.Shared.Features.Data.Models;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;
using TenberBot.Shared.Features.Results.Command;

namespace TenberBot.Features.FortuneFeature.Modules.Command;

[Remarks("Server Management")]
[RequireUserPermission(GuildPermission.ManageGuild)]
public class ManageGuildCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly IFortuneDataService fortuneDataService;
    private readonly IInteractionParentDataService interactionParentDataService;

    public ManageGuildCommandModule(
        IFortuneDataService fortuneDataService,
        IInteractionParentDataService interactionParentDataService)
    {
        this.fortuneDataService = fortuneDataService;
        this.interactionParentDataService = interactionParentDataService;
    }

    [Command("fortunes", ignoreExtraArgs: true)]
    [Summary("Manage random fortunes.")]
    public async Task<RuntimeResult> FortunesList()
    {
        var reply = await Context.Message.ReplyAsync(embed: await fortuneDataService.GetAllAsEmbed());

        await SetParent(InteractionParentType.Fortune, reply.Id);

        var components = new ComponentBuilder()
            .WithButton("Add", $"fortune:add,{reply.Id}", ButtonStyle.Success, new Emoji("➕"))
            .WithButton("Delete", $"fortune:delete,{reply.Id}", ButtonStyle.Danger, new Emoji("🗑"));

        await reply.ModifyAsync(x => x.Components = components.Build());

        return DeleteResult.FromSuccess();
    }

    private async Task SetParent(InteractionParentType parentType, ulong messageId)
    {
        var previousParent = await interactionParentDataService.Set(new InteractionParent
        {
            GuildId = Context.Guild.Id,
            ChannelId = Context.Channel.Id,
            UserId = null,
            InteractionParentType = parentType,
            MessageId = messageId,
        });

        await Context.Channel.GetAndModify(previousParent, x => x.Components = new ComponentBuilder().Build());
    }
}
