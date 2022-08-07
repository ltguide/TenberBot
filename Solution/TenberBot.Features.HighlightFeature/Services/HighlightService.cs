using Discord.WebSocket;
using TenberBot.Features.HighlightFeature.Data.Services;
using TenberBot.Shared.Features.Services;

namespace TenberBot.Features.HighlightFeature.Services;

public class HighlightService : IGuildMessageService
{
    private readonly IHighlightDataService highlightDataService;
    private readonly CacheService cacheService;

    public HighlightService(
        IHighlightDataService highlightDataService,
        CacheService cacheService)
    {
        this.highlightDataService = highlightDataService;
        this.cacheService = cacheService;
    }

    public Task Handle(SocketGuildChannel channel, SocketUserMessage message)
    {
        /*
            Pings people when a word is mentioned-filtered by smart, reg loose
            Could block people/channels from the list (if said by them or in a channel blocked it does not dm you)
            On the highlights we were hoping that people could have it separated for the SFW vs NSFW—> ephemeral 
         */

        //await (channel as SocketTextChannel).SendMessageAsync("mooo");

        return Task.CompletedTask;
    }
}
