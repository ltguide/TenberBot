using Discord.Interactions;
using Discord.WebSocket;

namespace TenberBot.Shared.Features.Extensions.DiscordInteractions;

public static class SocketInteractionContextExtensions
{
    public static SocketGuildUser? GetRandomUser(this SocketInteractionContext context)
    {
        if (context.Channel is not SocketTextChannel textChannel)
            return null;

        return textChannel.Users
            .Where(x => x.IsBot == false && x != context.User)
            .OrderBy(x => Guid.NewGuid())
            .FirstOrDefault();
    }
}
