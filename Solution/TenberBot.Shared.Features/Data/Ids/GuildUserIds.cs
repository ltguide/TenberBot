using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

namespace TenberBot.Shared.Features.Data.Ids;

public class GuildUserIds
{
    public ulong GuildId { get; set; }
    public ulong UserId { get; set; }

    public GuildUserIds(SocketInteractionContext context) : this(context.Guild, context.User) { }

    public GuildUserIds(SocketCommandContext context) : this(context.Guild, context.User) { }

    public GuildUserIds(SocketGuild guild, SocketUser user) : this(guild.Id, user.Id) { }

    public GuildUserIds(ulong guildId, ulong userId)
    {
        GuildId = guildId;
        UserId = userId;
    }

    public GuildUserIds() { }
}
