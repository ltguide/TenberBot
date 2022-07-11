using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TenberBot.Data.Settings.Server;
using TenberBot.Extensions;
using TenberBot.Services;

namespace TenberBot.Modules.Command;

[Remarks("Channel Management")]
public class ManageChannelCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly CacheService cacheService;
    private readonly ILogger<ManageChannelCommandModule> logger;

    public ManageChannelCommandModule(
        CacheService cacheService,
        ILogger<ManageChannelCommandModule> logger)
    {
        this.cacheService = cacheService;
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

        await Context.Message.AddReactionAsync(cacheService.Get<EmoteServerSettings>(Context.Guild).Busy);

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
