using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TenberBot.Data;
using TenberBot.Extensions;

namespace TenberBot.Modules.Command;

[Remarks("Channel Management")]
public class ManageChannelCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly ILogger<ManageChannelCommandModule> logger;

    public ManageChannelCommandModule(
        ILogger<ManageChannelCommandModule> logger)
    {
        this.logger = logger;
    }

    [Command("purge", ignoreExtraArgs: true)]
    [Summary("Remove messages from channel history.")]
    [Remarks("`<count>`")]
    [RequireUserPermission(ChannelPermission.ManageMessages)]
    [RequireBotPermission(ChannelPermission.ManageMessages)]
    public async Task Purge(int count)
    {
        if (Context.Channel is not SocketTextChannel channel)
            return;

        await Context.Message.AddReactionAsync(GlobalSettings.EmoteUnknown);

        _ = Task.Run(() => Process(channel, count));
    }

    private async Task Process(SocketTextChannel channel, int count)
    {
        var messages = (await channel.GetMessagesAsync(limit: Math.Min(Math.Max(1, count), 100) + 1).FlattenAsync()).Where(x => x.IsPinned == false).ToList();

        count = messages.Count - 1;

        var reply = await ReplyAsync($"I found {count} message{(count != 1 ? "s" : "")} to clean up...");

        await channel.DeleteMessagesAsync(messages);

        await reply.ModifyAsync(x => x.Content = $"💥 {reply.Content} and I'm all done! 💥");

        reply.DeleteSoon();
    }
}
