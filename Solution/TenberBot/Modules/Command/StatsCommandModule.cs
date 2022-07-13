using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TenberBot.Data.Services;
using TenberBot.Data.Settings.Server;
using TenberBot.Extensions;
using TenberBot.Extensions.ImageSharp;
using TenberBot.Results.Command;
using TenberBot.Services;
using Color = SixLabors.ImageSharp.Color;
using Image = SixLabors.ImageSharp.Image;

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

        using var memoryStream = new MemoryStream();

        using (var img = Image.Load(card.ImageData, out IImageFormat format))
        {
            img.Mutate(ctx => ctx.AddRankData(card, Context.Guild, Context.User, userLevel));

            if (myAvatar != null)
            {
                using var myAvatarImage = Image.Load(myAvatar);
                myAvatarImage.Mutate(ctx => ctx.Resize(60, 60).BackgroundColor(Color.Black));

                img.Mutate(ctx => ctx.DrawImage(myAvatarImage, new Point(140, 190), new GraphicsOptions { AlphaCompositionMode = PixelAlphaCompositionMode.DestOver }));
            }

            if (userAvatar != null)
            {
                using var userAvatarImage = Image.Load(userAvatar);
                userAvatarImage.Mutate(ctx => ctx.Resize(160, 160).ApplyRoundedCorners(80));

                img.Mutate(ctx => ctx.DrawImage(userAvatarImage, new Point(15, 40), new GraphicsOptions { AlphaCompositionMode = PixelAlphaCompositionMode.DestOver }));
            }

            img.Save(memoryStream, format);
        }

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
