using Discord;
using Discord.Commands;
using TenberBot.Features.RandomizerFeature.Data.Visuals;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;

namespace TenberBot.Features.RandomizerFeature.Modules.Command;

[Remarks("Randomized")]
[RequireBotPermission(ChannelPermission.SendMessages)]
public class RandomizedCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly IVisualDataService visualDataService;
    private readonly IUserStatDataService userStatDataService;

    public RandomizedCommandModule(
        IVisualDataService visualDataService,
        IUserStatDataService userStatDataService)
    {
        this.visualDataService = visualDataService;
        this.userStatDataService = userStatDataService;
    }

    [Command("coin", ignoreExtraArgs: true)]
    [Alias("coinflip", "coinflips")]
    [Summary("Flips a coin. Are you feeling lucky?")]
    public async Task Coin()
    {
        var footer = "No streak 😓";

        var userStats = await userStatDataService.GetOrAddByContext(Context);

        var flip = Random.Shared.Next(2);
        var visualType = flip == 0 ? Visuals.CoinHead : Visuals.CoinTail;

        var visual = await visualDataService.GetRandom(visualType);
        if (visual == null)
            return;

        if (flip != userStats.CoinFlipPrevious)
        {
            if (userStats.CoinFlipPrevious != null && userStats.CoinFlipStreak > 1)
                footer = $"Streak lost! Made it to {userStats.CoinFlipStreakText}";

            userStats.CoinFlipPrevious = flip;
            userStats.CoinFlipStreak = 1;
        }
        else
        {
            userStats.CoinFlipStreak++;

            footer = $"Current streak is {userStats.CoinFlipStreakText} in a row.";

            if (userStats.CoinFlipStreak > userStats.CoinFlipRecord)
            {
                userStats.CoinFlipRecord = userStats.CoinFlipStreak;

                footer += " Your personal best!";
            }
        }

        userStats.CoinFlips++;

        await userStatDataService.Save();

        var embedBuilder = new EmbedBuilder
        {
            Author = Context.User.GetEmbedAuthor("flips a coin"),
            Title = visualType == Visuals.CoinHead ? "Heads!" : "Tails!",
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
        var visual = await visualDataService.GetRandom(Visuals.EightBall);
        if (visual == null)
            return;

        var embedBuilder = new EmbedBuilder
        {
            Author = Context.User.GetEmbedAuthor("shakes the Magic 8 Ball"),
            Description = "\\**Magic 8 Ball ponders*\\*\n",
            ImageUrl = $"attachment://{visual.AttachmentFilename}",
        };

        if (question != null)
            embedBuilder.Description = $"> {question}{(question.EndsWith("?") ? "" : "?")}\n\n{embedBuilder.Description}";

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
