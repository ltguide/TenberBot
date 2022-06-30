using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenberBot.Extensions;

public static class SocketUserMessageExtensions
{
    public static MessageReference GetReferenceTo(this SocketUserMessage socketUserMessage)
    {
        return new MessageReference(socketUserMessage.Id);
    }
}
