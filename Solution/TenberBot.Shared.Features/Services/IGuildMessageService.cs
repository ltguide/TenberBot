using Discord.WebSocket;

namespace TenberBot.Shared.Features.Services;

public interface IGuildMessageService
{
    Task Handle(SocketGuildChannel channel, SocketUserMessage message);
}
