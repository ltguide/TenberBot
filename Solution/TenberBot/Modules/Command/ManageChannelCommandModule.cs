using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TenberBot.Data;

namespace TenberBot.Modules.Command;

public class ManageChannelCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly ILogger<ManageChannelCommandModule> logger;

    public ManageChannelCommandModule(
        ILogger<ManageChannelCommandModule> logger)
    {
        this.logger = logger;
    }

    [Command("purge")]
    [Summary("Purge messages from channel.")]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    [RequireBotPermission(ChannelPermission.ManageMessages)]
    public async Task Purge(int count, bool quiet = false)
    {
        if (Context.Channel is not SocketTextChannel channel)
            return;

        count = Math.Min(Math.Max(1, count), 100);

        await Context.Message.AddReactionAsync(GlobalSettings.EmoteUnknown);

        var messages = await Context.Channel.GetMessagesAsync(limit: count + 1).FlattenAsync();

        count = messages.Count() - 1;

        var content = $"Deleting {count} message{(count != 1 ? "s" : "")}...";
        var reply = await ReplyAsync(content);

        await channel.DeleteMessagesAsync(messages);

        if (quiet)
            await reply.DeleteAsync();
        else
        {
            await reply.ModifyAsync(x => x.Content = $"{content} all done!");
            await reply.AddReactionAsync(GlobalSettings.EmoteSuccess);
        }
    }


}
