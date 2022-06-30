using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TenberBot.Extensions;

public static class SocketUserExtensions
{
    public static string GetDisplayName(this SocketUser socketUser)
    {
        if (socketUser is SocketGuildUser socketGuildUser)
            return socketGuildUser.DisplayName;

        return socketUser.Username;
    }

    public static EmbedAuthorBuilder GetEmbedAuthor(this SocketUser socketUser, string? append = null)
    {
        return new EmbedAuthorBuilder {
            Name = $"{socketUser.GetDisplayName()} {append}",
            IconUrl = socketUser.GetAvatarUrl()
        };
    }
}
