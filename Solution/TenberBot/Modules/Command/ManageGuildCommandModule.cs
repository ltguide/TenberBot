using Discord;
using Discord.Commands;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Data.Services;
using TenberBot.Extensions;
using TenberBot.Results.Command;

namespace TenberBot.Modules.Command;

[Remarks("Server Management")]
[RequireUserPermission(GuildPermission.ManageGuild)]
public class ManageGuildCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly ISprintSnippetDataService sprintSnippetDataService;
    private readonly IHugDataService hugDataService;
    private readonly IGreetingDataService greetingDataService;
    private readonly IBotStatusDataService botStatusDataService;
    private readonly IInteractionParentDataService interactionParentDataService;
    private readonly ILogger<ManageGuildCommandModule> logger;

    public ManageGuildCommandModule(
        ISprintSnippetDataService sprintSnippetDataService,
        IHugDataService hugDataService,
        IGreetingDataService greetingDataService,
        IBotStatusDataService botStatusDataService,
        IInteractionParentDataService interactionParentDataService,
        ILogger<ManageGuildCommandModule> logger)
    {
        this.sprintSnippetDataService = sprintSnippetDataService;
        this.hugDataService = hugDataService;
        this.greetingDataService = greetingDataService;
        this.botStatusDataService = botStatusDataService;
        this.interactionParentDataService = interactionParentDataService;
        this.logger = logger;
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

    [Command("greetings", ignoreExtraArgs: true)]
    [Summary("Manage random greetings.")]
    [Remarks("`<GreetingType>`")]
    public async Task<RuntimeResult> GreetingsList(GreetingType? greetingType = null)
    {
        if (greetingType == null)
            return RemainResult.FromError($"Provide an option of: {string.Join(", ", Enum.GetNames<GreetingType>())}");

        var reply = await Context.Message.ReplyAsync(embed: await greetingDataService.GetAllAsEmbed(greetingType.Value));

        await SetParent(InteractionParentType.Greeting, reply.Id, greetingType);

        var components = new ComponentBuilder()
            .WithButton("Add", $"greeting:add,{reply.Id}", ButtonStyle.Success, new Emoji("➕"))
            .WithButton("Delete", $"greeting:delete,{reply.Id}", ButtonStyle.Danger, new Emoji("🗑"));

        await reply.ModifyAsync(x => x.Components = components.Build());

        return DeleteResult.FromSuccess();
    }

    [Command("hugs", ignoreExtraArgs: true)]
    [Summary("Manage random hugs.")]
    [Remarks("`<HugType>`")]
    public async Task<RuntimeResult> HugsList(HugType? hugType = null)
    {
        if (hugType == null)
            return RemainResult.FromError($"Provide an option of: {string.Join(", ", Enum.GetNames<HugType>())}");

        var reply = await Context.Message.ReplyAsync(embed: await hugDataService.GetAllAsEmbed(hugType.Value));

        await SetParent(InteractionParentType.Hug, reply.Id, hugType);

        var components = new ComponentBuilder()
            .WithButton("Add", $"hug:add,{reply.Id}", ButtonStyle.Success, new Emoji("➕"))
            .WithButton("Delete", $"hug:delete,{reply.Id}", ButtonStyle.Danger, new Emoji("🗑"));

        await reply.ModifyAsync(x => x.Components = components.Build());

        return DeleteResult.FromSuccess();
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
            ChannelId = Context.Channel.Id,
            UserId = null,
            InteractionParentType = parentType,
            MessageId = messageId,
            Reference = reference == null ? null : Convert.ToInt32(reference),
        });

        await Context.Channel.GetAndModify(previousParent, (x) => x.Components = new ComponentBuilder().Build());
    }
}
