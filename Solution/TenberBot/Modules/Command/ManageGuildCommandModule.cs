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
    private readonly IHighFiveDataService highFiveDataService;
    private readonly IPatDataService patDataService;
    private readonly IHugDataService hugDataService;
    private readonly IGreetingDataService greetingDataService;
    private readonly IBotStatusDataService botStatusDataService;
    private readonly IInteractionParentDataService interactionParentDataService;
    private readonly ILogger<ManageGuildCommandModule> logger;

    public ManageGuildCommandModule(
        ISprintSnippetDataService sprintSnippetDataService,
        IHighFiveDataService highFiveDataService,
        IPatDataService patDataService,
        IHugDataService hugDataService,
        IGreetingDataService greetingDataService,
        IBotStatusDataService botStatusDataService,
        IInteractionParentDataService interactionParentDataService,
        ILogger<ManageGuildCommandModule> logger)
    {
        this.sprintSnippetDataService = sprintSnippetDataService;
        this.highFiveDataService = highFiveDataService;
        this.patDataService = patDataService;
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

    [Command("pats", ignoreExtraArgs: true)]
    [Summary("Manage random pats.")]
    [Remarks("`<PatType>`")]
    public async Task<RuntimeResult> PatsList(PatType? patType = null)
    {
        if (patType == null)
            return RemainResult.FromError($"Provide an option of: {string.Join(", ", Enum.GetNames<PatType>())}");

        var reply = await Context.Message.ReplyAsync(embed: await patDataService.GetAllAsEmbed(patType.Value));

        await SetParent(InteractionParentType.Pat, reply.Id, patType);

        var components = new ComponentBuilder()
            .WithButton("Add", $"pat:add,{reply.Id}", ButtonStyle.Success, new Emoji("➕"))
            .WithButton("Delete", $"pat:delete,{reply.Id}", ButtonStyle.Danger, new Emoji("🗑"));

        await reply.ModifyAsync(x => x.Components = components.Build());

        return DeleteResult.FromSuccess();
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
