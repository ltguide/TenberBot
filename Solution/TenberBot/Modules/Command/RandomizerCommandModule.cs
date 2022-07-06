using Discord;
using Discord.Commands;
using TenberBot.Data.Enums;
using TenberBot.Data.Services;
using TenberBot.Extensions;

namespace TenberBot.Modules.Command;

[Remarks("Randomized")]
[RequireBotPermission(ChannelPermission.SendMessages)]
public class RandomizedCommandModule : ModuleBase<SocketCommandContext>
{
    private static int CoinFlipCounter = 0;
    private static VisualType? CoinFlipPrevious = null;

    private readonly IVisualDataService visualDataService;
    private readonly ILogger<RandomizedCommandModule> logger;

    public RandomizedCommandModule(
        IVisualDataService visualDataService,
        ILogger<RandomizedCommandModule> logger)
    {
        this.visualDataService = visualDataService;
        this.logger = logger;
    }

    [Command("coin", ignoreExtraArgs: true)]
    [Alias("coinflip", "coinflips")]
    [Summary("Flips a coin. Are you feeling lucky?")]
    public async Task Coin()
    {
        var footer = "No streak 😓";

        var visualType = Random.Shared.Next(2) == 0 ? VisualType.CoinHead : VisualType.CoinTail;

        if (visualType != CoinFlipPrevious)
        {
            if (CoinFlipPrevious != null && CoinFlipCounter > 1)
                footer = $"Streak lost! Made it to {CoinFlipCounter} flip{(CoinFlipCounter != 1 ? "s" : "")}";

            CoinFlipPrevious = visualType;
            CoinFlipCounter = 1;
        }
        else
        {
            CoinFlipCounter++;

            footer = $"Current streak is {CoinFlipCounter} flip{(CoinFlipCounter != 1 ? "s" : "")} in a row.";
        }

        var visual = await visualDataService.GetRandom(visualType);
        if (visual == null)
            return;

        var embedBuilder = new EmbedBuilder
        {
            Title = visualType == VisualType.CoinHead ? "Heads!" : "Tails!",
            ThumbnailUrl = $"attachment://{visual.AttachmentFilename}",
        }.WithFooter(footer);

        await Context.Channel.SendFileAsync(
            visual.Stream,
            visual.AttachmentFilename,
            embed: embedBuilder.Build(),
            messageReference: Context.Message.GetReferenceTo());
    }

    [Command("8ball")]
    [Summary("Shakes the Magic 8 Ball.")]
    [Remarks("`[yes/no question]`")]
    public async Task EightBall([Remainder] string? question = null)
    {
        var visual = await visualDataService.GetRandom(VisualType.EightBall);
        if (visual == null)
            return;

        var embedBuilder = new EmbedBuilder
        {
            Author = Context.User.GetEmbedAuthor("shook the Magic 8 Ball"),
            Description = @"*\*Magic 8 Ball ponders\**",
            ImageUrl = $"attachment://{visual.AttachmentFilename}",
        };

        if (question != null)
            embedBuilder.WithTitle($"You ask: {question}{(question.EndsWith("?") ? "" : "?")}");

        await Context.Channel.SendFileAsync(
            visual.Stream,
            visual.AttachmentFilename,
            embed: embedBuilder.Build(),
            messageReference: Context.Message.GetReferenceTo());
    }

    /*
!choose <value 1, value 2, value 3 ... | value1 value2 value3... >
Chooses one thing out of a list of many - useful for making hard decisions, but I can't guarantee I'll make the right ones!
!roll <die>
Roll a die! (or multiple). Behaves like !choose if dice not specified.
!8ball [question]
Ask the magic eight-ball. Be warned - you may not hear the answer you are wanting.
     */

}
