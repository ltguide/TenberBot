using Discord;
using Discord.Commands;
using TenberBot.Features.SprintFeature.Data.Enums;
using TenberBot.Features.SprintFeature.Data.Services;
using TenberBot.Shared.Features.Data.Enums;
using TenberBot.Shared.Features.Data.Models;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;
using TenberBot.Shared.Features.Results.Command;

namespace TenberBot.Features.SprintFeature.Modules.Command;

[Remarks("Server Management")]
[RequireUserPermission(GuildPermission.ManageGuild)]
public class ManageGuildCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly ISprintSnippetDataService sprintSnippetDataService;
    private readonly IInteractionParentDataService interactionParentDataService;

    public ManageGuildCommandModule(
        ISprintSnippetDataService sprintSnippetDataService,
        IInteractionParentDataService interactionParentDataService)
    {
        this.sprintSnippetDataService = sprintSnippetDataService;
        this.interactionParentDataService = interactionParentDataService;
    }

    [Command("sprint-snippets", ignoreExtraArgs: true)]
    [Summary("Manage random sprint snippets.")]
    [Remarks("`<SprintSnippetType>`")]
    public async Task<RuntimeResult> SprintSnippetsList(SprintSnippetType? sprintSnippetType = null)
    {
        if (sprintSnippetType == null)
            return RemainResult.FromError($"Provide an option of: {string.Join(", ", Enum.GetNames<SprintSnippetType>())}");

        var reply = await Context.Message.ReplyAsync(embed: await sprintSnippetDataService.GetAllAsEmbed(sprintSnippetType.Value));

        await SetParent(InteractionParentType.SprintSnippet, reply.Id, sprintSnippetType);

        var components = new ComponentBuilder()
            .WithButton("Add", $"sprint-snippet:add,{reply.Id}", ButtonStyle.Success, new Emoji("➕"))
            .WithButton("Delete", $"sprint-snippet:delete,{reply.Id}", ButtonStyle.Danger, new Emoji("🗑"));

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
