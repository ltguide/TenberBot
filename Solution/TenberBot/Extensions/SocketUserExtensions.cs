using Discord;
using Discord.WebSocket;

namespace TenberBot.Extensions;

public static class SocketUserExtensions
{
    public static string GetMention(this SocketUser socketUser)
    {
        return socketUser.Id.GetUserMention();
    }

    public static string GetDisplayName(this SocketUser socketUser)
    {
        if (socketUser is SocketGuildUser socketGuildUser)
            return socketGuildUser.DisplayName;

        return socketUser.Username;
    }

    public static string GetDisplayNameSanitized(this SocketUser socketUser)
    {
        return GetDisplayName(socketUser).SanitizeMD();
    }

    public static EmbedAuthorBuilder GetEmbedAuthor(this SocketUser socketUser, string? append = null)
    {
        return new EmbedAuthorBuilder
        {
            Name = $"{socketUser.GetDisplayName()}{(append?.StartsWith("'") == false ? " " : "")}{append}",
            IconUrl = socketUser.GetCurrentAvatarUrl()
        };
    }

    public static string GetCurrentAvatarUrl(this SocketUser socketUser)
    {
        return (socketUser as SocketGuildUser)?.GetGuildAvatarUrl() ?? socketUser.GetAvatarUrl() ?? socketUser.GetDefaultAvatarUrl();
    }
}
