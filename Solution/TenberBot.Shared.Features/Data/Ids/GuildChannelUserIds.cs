using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

namespace TenberBot.Shared.Features.Data.Ids;

public class GuildChannelUserIds
{
    public ulong GuildId { get; set; }
    public ulong ChannelId { get; set; }
    public ulong UserId { get; set; }

    public GuildChannelUserIds(SocketInteractionContext context) : this(context.Guild, context.Channel, context.User) { }

    public GuildChannelUserIds(SocketCommandContext context) : this(context.Guild, context.Channel, context.User) { }

    public GuildChannelUserIds(SocketGuild guild, ISocketMessageChannel channel, SocketUser user) : this(guild.Id, channel.Id, user.Id) { }

    public GuildChannelUserIds(ulong guildId, ulong channelId, ulong userId)
    {
        GuildId = guildId;
        ChannelId = channelId;
        UserId = userId;
    }

    public GuildChannelUserIds() { }
}
