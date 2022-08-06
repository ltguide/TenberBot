using Discord;
using Discord.WebSocket;

namespace TenberBot.Shared.Features.Extensions.DiscordWebSocket;

public static class SocketUserMessageExtensions
{
    public static MessageReference GetReferenceTo(this SocketUserMessage socketUserMessage)
    {
        return new MessageReference(socketUserMessage.Id);
    }
}
