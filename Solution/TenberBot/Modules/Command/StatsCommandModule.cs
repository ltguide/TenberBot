using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TenberBot.Data.Services;
using TenberBot.Data.Settings.Server;
using TenberBot.Extensions;
using TenberBot.Helpers;
using TenberBot.Results.Command;
using TenberBot.Services;

namespace TenberBot.Modules.Command;

[Remarks("Information")]
public class StatsCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly IUserLevelDataService userLevelDataService;
    private readonly WebService webService;
    private readonly DiscordSocketClient client;
    private readonly CacheService cacheService;
    private readonly ILogger<StatsCommandModule> logger;

    public StatsCommandModule(
        IUserLevelDataService userLevelDataService,
        WebService webService,
        DiscordSocketClient client,
        CacheService cacheService,
        ILogger<StatsCommandModule> logger)
    {
        this.userLevelDataService = userLevelDataService;
        this.webService = webService;
        this.client = client;
        this.cacheService = cacheService;
        this.logger = logger;
    }

    [Command("rank", ignoreExtraArgs: true)]
    [Alias("lvl", "level")]
    [Summary("See your experience information.")]
    public async Task<RuntimeResult> Rank()
    {
        await Context.Message.AddReactionAsync(cacheService.Get<EmoteServerSettings>(Context.Guild).Busy);

        var userLevel = await userLevelDataService.GetByContext(Context);
        if (userLevel == null)
            return DeleteResult.FromError("You have no experience right now.");

        var card = FindCard();
        if (card == null)
            return DeleteResult.FromError("Unable to find a configured rank card.");

        await userLevelDataService.GetRanks(userLevel);

        var myAvatar = await webService.GetBytes(client.CurrentUser.GetCurrentAvatarUrl(), TimeSpan.FromMinutes(60));
        if (myAvatar == null)
            return DeleteResult.FromError("Failed to load my avatar. Please try again.");

        var userAvatar = await webService.GetBytes(Context.User.GetCurrentAvatarUrl(), TimeSpan.FromMinutes(5));
        if (userAvatar == null)
            return DeleteResult.FromError("Failed to load your avatar. Please try again.");

        using var memoryStream = RankCardHelper.GetStream(card, Context.Guild, Context.User, userLevel, myAvatar, userAvatar);

        await Context.Channel.SendFileAsync(new FileAttachment(memoryStream, $"{Context.User.Id}_{card.ImageName}"), messageReference: Context.Message.GetReferenceTo());

        await Context.Message.RemoveAllReactionsAsync();

        return DeleteResult.FromSuccess("");
    }

    private RankCardSettings? FindCard()
    {
        if (Context.User is not SocketGuildUser user)
            return null;

        var cards = cacheService.Get<RankServerSettings>(Context.Guild).Cards.Where(x => x.ImageData != null).ToDictionary(x => x.Role, x => x);
        if (cards.Count == 0)
            return null;

        foreach (var role in user.Roles.Where(x => x.Mention != "@everyone"))
        {
            if (cards.TryGetValue(role.Mention, out var card))
                return card;
        }

        return cards.TryGetValue("@everyone", out var everyoneCard) ? everyoneCard : cards.First().Value;
    }
}
