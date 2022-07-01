using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace TenberBot.Extensions;

public static class ISocketMessageChannelExtensions
{
    public static async Task ModifyEmbed(this ISocketMessageChannel socketMessageChannel, ulong messageId, EmbedBuilder embedBuilder)
    {
        var message = await socketMessageChannel.GetMessageAsync(messageId);

        if (message is RestUserMessage restUserMessage)
            await restUserMessage.ModifyAsync(x => x.Embed = embedBuilder.Build());

        if (message is SocketUserMessage socketUserMessage)
            await socketUserMessage.ModifyAsync(x => x.Embed = embedBuilder.Build());
    }
}
