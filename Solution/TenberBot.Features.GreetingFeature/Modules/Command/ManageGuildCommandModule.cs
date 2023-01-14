using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using TenberBot.Features.GreetingFeature.Data.Enums;
using TenberBot.Features.GreetingFeature.Data.InteractionParents;
using TenberBot.Features.GreetingFeature.Data.Services;
using TenberBot.Shared.Features.Data.Models;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;
using TenberBot.Shared.Features.Results.Command;

namespace TenberBot.Features.GreetingFeature.Modules.Command;

[Remarks("Server Management")]
[RequireUserPermission(GuildPermission.ManageGuild)]
public class ManageGuildCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly IGreetingDataService greetingDataService;
    private readonly IInteractionParentDataService interactionParentDataService;
    private readonly ILogger<ManageGuildCommandModule> logger;

    public ManageGuildCommandModule(
        IGreetingDataService greetingDataService,
        IInteractionParentDataService interactionParentDataService,
        ILogger<ManageGuildCommandModule> logger)
    {
        this.greetingDataService = greetingDataService;
        this.interactionParentDataService = interactionParentDataService;
        this.logger = logger;
    }

    [Command("greetings", ignoreExtraArgs: true)]
    [Summary("Manage random greetings.")]
    [Remarks("`<GreetingType>`")]
    public async Task<RuntimeResult> GreetingsList(GreetingType? greetingType = null)
    {
        if (greetingType == null)
            return RemainResult.FromError($"Provide an option of: {string.Join(", ", Enum.GetNames<GreetingType>())}");

        var reply = await Context.Message.ReplyAsync(embed: await greetingDataService.GetAllAsEmbed(greetingType.Value));

        await SetParent(InteractionParents.Embed, reply.Id, greetingType);

        var components = new ComponentBuilder()
            .WithButton("Add", $"greeting:add,{reply.Id}", ButtonStyle.Success, new Emoji("➕"))
            .WithButton("Delete", $"greeting:delete,{reply.Id}", ButtonStyle.Danger, new Emoji("🗑"));

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
