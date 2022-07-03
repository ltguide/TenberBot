using Discord;
using Discord.Interactions;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Data.Services;

namespace TenberBot.Modules.Interaction;

[Group("channel-setting", "Manage channel settings for the bot.")]
[RequireUserPermission(ChannelPermission.ManageChannels)]
public class ChannelSettingInteractionModule : InteractionModuleBase<SocketInteractionContext>
{

    private readonly ISprintDataService sprintDataService;

    public ChannelSettingInteractionModule(
        ISprintDataService sprintDataService)
    {
        this.sprintDataService = sprintDataService;
    }

    [SlashCommand("sprint", "Set the prefix for message commands.")]
    public async Task SetSprint(SprintMode mode, IRole role)
    {
        var sprintChannel = await sprintDataService.GetChannelById(Context.Channel.Id);

        var newObject = new SprintChannel
        {
            ChannelId = Context.Channel.Id,
            SprintMode = mode,
            Role = role.Mention
        };

        if (sprintChannel == null)
            await sprintDataService.Add(newObject);
        else
            await sprintDataService.Update(sprintChannel, newObject);

        await RespondAsync($"Channel settings for *sprint* updated.\n\n> **Mode**: {mode}\n> **Role**: {role}");
    }
}
