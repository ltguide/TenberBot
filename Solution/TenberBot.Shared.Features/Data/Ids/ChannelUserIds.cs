using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

namespace TenberBot.Shared.Features.Data.Ids;

public class ChannelUserIds
{
    public ulong ChannelId { get; set; }
    public ulong UserId { get; set; }

    public ChannelUserIds(SocketInteractionContext context) : this(context.Channel, context.User) { }

    public ChannelUserIds(SocketCommandContext context) : this(context.Channel, context.User) { }

    public ChannelUserIds(ISocketMessageChannel channel, SocketUser user) : this(channel.Id, user.Id) { }

    public ChannelUserIds(ulong channelId, ulong userId)
    {
        ChannelId = channelId;
        UserId = userId;
    }

    public ChannelUserIds() { }
}
