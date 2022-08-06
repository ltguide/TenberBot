using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace TenberBot.Shared.Features.Extensions.DiscordWebSocket;

public static class ISocketMessageChannelExtensions
{
    public static async Task GetAndModify(this ISocketMessageChannel socketMessageChannel, ulong? messageId, Action<MessageProperties> action)
    {
        if (messageId == null)
            return;

        var message = await socketMessageChannel.GetMessageAsync(messageId.Value);
        if (message == null)
            return;

        if (message is RestUserMessage restUserMessage)
            await restUserMessage.ModifyAsync(action);

        if (message is SocketUserMessage socketUserMessage)
            await socketUserMessage.ModifyAsync(action);
    }
}
