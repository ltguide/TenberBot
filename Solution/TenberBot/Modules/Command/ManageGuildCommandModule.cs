using Discord;
using Discord.Commands;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Data.Services;
using TenberBot.Extensions;
using TenberBot.Results.Command;

namespace TenberBot.Modules.Command;

[RequireUserPermission(GuildPermission.ManageGuild)]
public class ManageGuildCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly IHugDataService hugDataService;
    private readonly IGreetingDataService greetingDataService;
    private readonly IBotStatusDataService botStatusDataService;
    private readonly IInteractionParentDataService interactionParentDataService;
    private readonly ILogger<ManageGuildCommandModule> logger;

    public ManageGuildCommandModule(
        IHugDataService hugDataService,
        IGreetingDataService greetingDataService,
        IBotStatusDataService botStatusDataService,
        IInteractionParentDataService interactionParentDataService,
        ILogger<ManageGuildCommandModule> logger)
    {
        this.hugDataService = hugDataService;
        this.greetingDataService = greetingDataService;
        this.botStatusDataService = botStatusDataService;
        this.interactionParentDataService = interactionParentDataService;
        this.logger = logger;
    }

    [Command("botstatuses")]
    [Summary("Manage random bot statuses.")]
    public async Task BotStatusesList()
    {
        var reply = await Context.Message.ReplyAsync(embed: (await botStatusDataService.GetAllAsEmbed()).Build());

        await SetInteractionParent(InteractionParentType.BotStatus, reply.Id);

        var components = new ComponentBuilder()
            .WithButton("Add", $"botstatus:add,{reply.Id}", ButtonStyle.Success, new Emoji("➕"))
            .WithButton("Delete", $"botstatus:delete,{reply.Id}", ButtonStyle.Danger, new Emoji("🗑"));

        await reply.ModifyAsync(x => x.Components = components.Build());
    }

    [Command("greetings")]
    [Summary("Manage random greetings.")]
    public async Task<RuntimeResult> GreetingsList(GreetingType? greetingType = null)
    {
        if (greetingType == null)
            return CustomResult.FromError($"Provide an option of: {string.Join(", ", Enum.GetNames<GreetingType>())}");

        var reply = await Context.Message.ReplyAsync(embed: (await greetingDataService.GetAllAsEmbed(greetingType.Value)).Build());

        await SetInteractionParent(InteractionParentType.Greeting, reply.Id);

        var components = new ComponentBuilder()
            .WithButton("Add", $"greeting:add,{greetingType},{reply.Id}", ButtonStyle.Success, new Emoji("➕"))
            .WithButton("Delete", $"greeting:delete,{greetingType},{reply.Id}", ButtonStyle.Danger, new Emoji("🗑"));

        await reply.ModifyAsync(x => x.Components = components.Build());

        return CustomResult.FromSuccess();
    }

    [Command("hugs")]
    [Summary("Manage random hugs.")]
    public async Task<RuntimeResult> HugsList(HugType? hugType = null)
    {
        if (hugType == null)
            return CustomResult.FromError($"Provide an option of: {string.Join(", ", Enum.GetNames<HugType>())}");

        var reply = await Context.Message.ReplyAsync(embed: (await hugDataService.GetAllAsEmbed(hugType.Value)).Build());

        await SetInteractionParent(InteractionParentType.Hug, reply.Id);

        var components = new ComponentBuilder()
            .WithButton("Add", $"hug:add,{hugType},{reply.Id}", ButtonStyle.Success, new Emoji("➕"))
            .WithButton("Delete", $"hug:delete,{hugType},{reply.Id}", ButtonStyle.Danger, new Emoji("🗑"));

        await reply.ModifyAsync(x => x.Components = components.Build());

        return CustomResult.FromSuccess();
    }

    private async Task SetInteractionParent(InteractionParentType parentType, ulong messageId)
    {
        var parent = await interactionParentDataService.GetByContext(parentType, Context);
        if (parent != null)
        {
            await Context.Channel.ModifyComponents(parent.MessageId, new ComponentBuilder());

            await interactionParentDataService.Update(parent, messageId, Context.User.Id);
        }
        else
            await interactionParentDataService.Add(new InteractionParent(Context) { InteractionParentType = parentType, MessageId = messageId, });
    }
}
