using Discord.Commands;
using Discord.WebSocket;

namespace TenberBot.Extensions;

public static class SocketCommandContextExtensions
{
    public static SocketGuildUser? GetRandomUser(this SocketCommandContext context)
    {
        if (context.Channel is not SocketTextChannel textChannel)
            return null;

        return textChannel.Users
            .Where(x => x.IsBot == false && x != context.User)
            .OrderBy(x => Guid.NewGuid())
            .FirstOrDefault();
    }
}
