using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

namespace TenberBot.Shared.Features.Data.Ids;

public class GuildChannelIds
{
    public ulong GuildId { get; set; }
    public ulong ChannelId { get; set; }

    public GuildChannelIds(SocketInteractionContext context) : this(context.Guild, context.Channel) { }

    public GuildChannelIds(SocketCommandContext context) : this(context.Guild, context.Channel) { }

    public GuildChannelIds(SocketGuild guild, ISocketMessageChannel channel) : this(guild.Id, channel.Id) { }

    public GuildChannelIds(ulong guildId, ulong channelId)
    {
        GuildId = guildId;
        ChannelId = channelId;
    }

    public GuildChannelIds() { }
}
