using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Data.POCO;
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
    private readonly IInteractionParentDataService interactionParentDataService;
    private readonly IUserLevelDataService userLevelDataService;
    private readonly WebService webService;
    private readonly IRankCardDataService rankCardDataService;
    private readonly CacheService cacheService;
    private readonly ILogger<StatsCommandModule> logger;

    public StatsCommandModule(
        IInteractionParentDataService interactionParentDataService,
        IUserLevelDataService userLevelDataService,
        WebService webService,
        IRankCardDataService rankCardDataService,
        CacheService cacheService,
        ILogger<StatsCommandModule> logger)
    {
        this.interactionParentDataService = interactionParentDataService;
        this.userLevelDataService = userLevelDataService;
        this.webService = webService;
        this.rankCardDataService = rankCardDataService;
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

        var card = await FindCard();
        if (card == null)
            return DeleteResult.FromError("Unable to find a configured rank card.");

        var myAvatar = await webService.GetBytes(Context.Client.CurrentUser.GetCurrentAvatarUrl(), TimeSpan.FromMinutes(60));
        if (myAvatar == null)
            return DeleteResult.FromError("Failed to load my avatar. Please try again.");

        var userAvatar = await webService.GetBytes(Context.User.GetCurrentAvatarUrl(), TimeSpan.FromMinutes(5));
        if (userAvatar == null)
            return DeleteResult.FromError("Failed to load your avatar. Please try again.");

        await userLevelDataService.LoadRanks(userLevel);

        using var memoryStream = RankCardHelper.GetStream(card, Context.Guild, Context.User, userLevel, myAvatar, userAvatar);

        await Context.Channel.SendFileAsync(new FileAttachment(memoryStream, $"{Context.User.Id}_{card.Filename}"), messageReference: Context.Message.GetReferenceTo());

        await Context.Message.RemoveAllReactionsAsync();

        return DeleteResult.FromSuccess("");
    }

    [Command("leaderboard", ignoreExtraArgs: true)]
    [Alias("lb")]
    [Summary("View experience leaderboard.")]
    public async Task Leaderboard()
    {
        await Context.Message.AddReactionAsync(cacheService.Get<EmoteServerSettings>(Context.Guild).Success);

        var reply = await Context.Message.ReplyAsync("Which leaderboard do you want to view?");

        var parent = await interactionParentDataService.Set(new InteractionParent
        {
            GuildId = Context.Guild.Id,
            ChannelId = Context.Channel.Id,
            UserId = Context.User.Id,
            InteractionParentType = InteractionParentType.Leaderboard,
            MessageId = reply.Id,
        }.SetReference(new LeaderboardView()));

        try
        {
            if (parent != null)
                await Context.Channel.DeleteMessageAsync(parent.Value);
        }
        catch (Exception) { }

        var components = new ComponentBuilder()
            .WithButton("Message", $"leaderboard:view-message,{reply.Id}", emote: new Emoji("📝"))
            .WithButton("Voice", $"leaderboard:view-voice,{reply.Id}", emote: new Emoji("🎤"));

        var settings = cacheService.Get<LeaderboardServerSettings>(Context.Guild);
        if (settings.DisplayEvent)
            components.WithButton("Event", $"leaderboard:view-event,{reply.Id}", emote: new Emoji("🎟"));

        await reply.ModifyAsync(x => x.Components = components.Build());
    }

    private async Task<RankCard?> FindCard()
    {
        if (Context.User is not SocketGuildUser user)
            return null;

        var cards = (await rankCardDataService.GetAllByGuildId(Context.Guild.Id)).Where(x => x.Data != null).ToDictionary(x => x.RoleId, x => x);
        if (cards.Count == 0)
            return null;

        foreach (var role in user.Roles.OrderByDescending(x => x.Position))
        {
            if (cards.TryGetValue(role.Id, out var card))
            {
                card.Name = role.Name;
                return card;
            }
        }

        return cards.First().Value;
    }
}
