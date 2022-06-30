using Discord;
using Discord.WebSocket;

namespace TenberBot.Extensions;

public static class SocketUserMessageExtensions
{
    public static MessageReference GetReferenceTo(this SocketUserMessage socketUserMessage)
    {
        return new MessageReference(socketUserMessage.Id);
    }
}
