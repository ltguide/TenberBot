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
        var settings = cacheService.Get<RankServerSettings>(Context.Guild);
        if (settings.BackgroundData == null)
            return DeleteResult.FromError("The rank background isn't configured.");

        var userLevel = await userLevelDataService.GetByContext(Context);
        if (userLevel == null)
            return DeleteResult.FromError("You have no experience right now.");

        await Context.Message.AddReactionAsync(cacheService.Get<EmoteServerSettings>(Context.Guild).Busy);

        await userLevelDataService.GetRanks(userLevel);

        var myAvatar = await webService.GetFileAttachment(client.CurrentUser.GetCurrentAvatarUrl());
        if (myAvatar == null)
            return DeleteResult.FromError("Failed to load my avatar. Please try again.");

        var userAvatar = await webService.GetFileAttachment(Context.User.GetCurrentAvatarUrl());
        if (userAvatar == null)
            return DeleteResult.FromError("Failed to load your avatar. Please try again.");

        using var memoryStream = new MemoryStream();

        using (var img = Image.Load(settings.BackgroundData, out IImageFormat format))
        {
            img.Mutate(ctx => ctx.AddRankData(settings, Context.Guild, Context.User, userLevel));

            if (myAvatar != null)
            {
                using var myAvatarImage = Image.Load(myAvatar.Value.Stream);
                myAvatarImage.Mutate(ctx => ctx.Resize(80, 80).BackgroundColor(Color.Black));

                img.Mutate(ctx => ctx.DrawImage(myAvatarImage, new Point(240, 270), new GraphicsOptions { AlphaCompositionMode = PixelAlphaCompositionMode.DestOver }));
            }

            if (userAvatar != null)
            {
                using var userAvatarImage = Image.Load(userAvatar.Value.Stream);
                userAvatarImage.Mutate(ctx => ctx.Resize(280, 280).ApplyRoundedCorners(100));

                img.Mutate(ctx => ctx.DrawImage(userAvatarImage, new Point(15, 15), new GraphicsOptions { AlphaCompositionMode = PixelAlphaCompositionMode.DestOver }));
            }

            img.Save(memoryStream, format);
        }

        await Context.Channel.SendFileAsync(new FileAttachment(memoryStream, $"{Context.User.Id}_{settings.BackgroundName}"), messageReference: Context.Message.GetReferenceTo());

        await Context.Message.RemoveAllReactionsAsync();

        return DeleteResult.FromSuccess("");
    }
}
