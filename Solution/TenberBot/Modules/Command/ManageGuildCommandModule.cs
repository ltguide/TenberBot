using Discord;
using Discord.Commands;
using TenberBot.Data.Enums;
using TenberBot.Data.Services;
using TenberBot.Extensions;
using TenberBot.Results.Command;

namespace TenberBot.Modules.Command;

[RequireUserPermission(GuildPermission.ManageGuild)]
public class ManageGuildCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly IGreetingDataService greetingDataService;
    private readonly IBotStatusDataService botStatusDataService;
    private readonly ILogger<ManageGuildCommandModule> logger;

    public ManageGuildCommandModule(
        IGreetingDataService greetingDataService,
        IBotStatusDataService botStatusDataService,
        ILogger<ManageGuildCommandModule> logger)
    {
        this.greetingDataService = greetingDataService;
        this.botStatusDataService = botStatusDataService;
        this.logger = logger;
    }

    [Command("botstatuses")]
    [Summary("Manage random bot statuses.")]
    public async Task BotStatusesList()
    {
        var reply = await Context.Message.ReplyToAsync(embed: (await botStatusDataService.GetAllAsEmbed()).Build());

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

        var reply = await Context.Message.ReplyToAsync(embed: (await greetingDataService.GetAllAsEmbed(greetingType.Value)).Build());

        var components = new ComponentBuilder()
            .WithButton("Add", $"greeting:add,{greetingType},{reply.Id}", ButtonStyle.Success, new Emoji("➕"))
            .WithButton("Delete", $"greeting:delete,{greetingType},{reply.Id}", ButtonStyle.Danger, new Emoji("🗑"));

        await reply.ModifyAsync(x => x.Components = components.Build());

        return CustomResult.FromSuccess();
    }
}
