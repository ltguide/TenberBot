using Discord.WebSocket;

namespace TenberBot.Extensions;

public static class DiscordSocketClientExtensions
{
    public static string GetCurrentAvatarUrl(this DiscordSocketClient client)
    {
        return client.CurrentUser.GetAvatarUrl() ?? client.CurrentUser.GetDefaultAvatarUrl();
    }
}
