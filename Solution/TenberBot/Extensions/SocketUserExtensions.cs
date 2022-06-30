using Discord;
using Discord.WebSocket;

namespace TenberBot.Extensions;

public static class SocketUserExtensions
{
    private static string GetDisplayName(this SocketUser socketUser)
    {
        if (socketUser is SocketGuildUser socketGuildUser)
            return socketGuildUser.DisplayName;

        return socketUser.Username;
    }

    public static string GetDisplayName(this SocketUser socketUser, bool sanitize = true)
    {
        if (sanitize)
            return Format.Sanitize(socketUser.GetDisplayName());

        return socketUser.GetDisplayName();
    }

    public static EmbedAuthorBuilder GetEmbedAuthor(this SocketUser socketUser, string? append = null)
    {
        return new EmbedAuthorBuilder
        {
            Name = $"{socketUser.GetDisplayName()} {append}",
            IconUrl = socketUser.GetAvatarUrl()
        };
    }
}
