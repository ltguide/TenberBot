using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Text.RegularExpressions;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Data.Services;
using TenberBot.Extensions;

namespace TenberBot.Modules.Command;

[RequireBotPermission(ChannelPermission.SendMessages)]
public class HighFiveCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly static Regex RecipientVariables = new(@"%user%|%recipient%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private readonly static Regex SelfVariables = new(@"%user%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private readonly static Regex StatVariables = new(@"%count%|%s%|%es%", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly IHighFiveDataService highFiveDataService;
    private readonly IVisualDataService visualDataService;
    private readonly IUserStatDataService userStatDataService;
    private readonly ILogger<HighFiveCommandModule> logger;

    public HighFiveCommandModule(
        IHighFiveDataService highFiveDataService,
        IVisualDataService visualDataService,
        IUserStatDataService userStatDataService,
        ILogger<HighFiveCommandModule> logger)
    {
        this.highFiveDataService = highFiveDataService;
        this.visualDataService = visualDataService;
        this.userStatDataService = userStatDataService;
        this.logger = logger;
    }

    [Command("high-five")]
    [Alias("highfive", "high5", "hi5")]
    [Summary("Up high!\n*If you reply to a user, `<user>` is no longer required.*")]
    [Remarks("`<user>` `[message]`")]
    public async Task HighFive([Remainder] string? message = null)
    {
        var recipient = Context.Message.MentionedUsers.FirstOrDefault();
        var highFiveType = (recipient == null || recipient == Context.User) ? VisualType.HighFiveSelf : VisualType.HighFive;

        var visual = await visualDataService.GetRandom(highFiveType);
        if (visual == null)
            return;

        EmbedBuilder embedBuilder;

        if (highFiveType == VisualType.HighFiveSelf)
        {
            var highFive = await highFiveDataService.GetRandom(HighFiveType.Self);

            if (highFive == null)
                return;

            embedBuilder = GetSelfEmbed(highFive);
        }
        else
        {
            var highFive = await highFiveDataService.GetRandom(HighFiveType.Recipient);
            var stat = await highFiveDataService.GetRandom(HighFiveType.Stat);

            if (highFive == null || stat == null)
                return;

            ++(await userStatDataService.GetOrAddByContext(Context)).HighFivesGiven;

            var received = ++(await userStatDataService.GetOrAddByIds(Context.Guild.Id, recipient!.Id)).HighFivesReceived;

            await userStatDataService.Save();

            embedBuilder = GetRecipientEmbed(recipient, highFive, stat, received);
        }


        if (recipient != null)
        {
            message = message?.Replace(recipient.GetMention(), "");

            if (string.IsNullOrWhiteSpace(message) == false)
                embedBuilder.AddField("\u200B", $"They wanted to say: {message}");
        }

        embedBuilder.WithImageUrl($"attachment://{visual.AttachmentFilename}");

        await Context.Channel.SendFileAsync(
            visual.Stream,
            visual.AttachmentFilename,
            embed: embedBuilder.Build(),
            messageReference: Context.Message.GetReferenceTo(),
            allowedMentions: AllowedMentions.None);
    }

    private EmbedBuilder GetRecipientEmbed(SocketUser recipient, HighFive highFive, HighFive stat, int count)
    {
        var highFiveText = RecipientVariables.Replace(highFive.Text, (match) =>
        {
            return match.Value.ToLower() switch
            {
                "%recipient%" => recipient.GetMention(),
                "%user%" => Context.User.GetMention(),
                _ => match.Value,
            };
        });

        var statText = StatVariables.Replace(stat.Text, (match) =>
        {
            return match.Value.ToLower() switch
            {
                "%count%" => count.ToString("N0"),
                "%s%" => count != 1 ? "s" : "",
                "%es%" => count != 1 ? "es" : "",
                _ => match.Value,
            };
        });

        return new EmbedBuilder
        {
            Author = Context.User.GetEmbedAuthor("is giving out high fives!"),
            Description = $"{highFiveText}\n\n{statText}",
            Color = Color.Green,
        };
    }

    private EmbedBuilder GetSelfEmbed(HighFive highFive)
    {
        var highFiveText = SelfVariables.Replace(highFive.Text, (match) =>
        {
            return match.Value.ToLower() switch
            {
                "%user%" => Context.User.GetMention(),
                _ => match.Value,
            };
        });

        return new EmbedBuilder
        {
            Author = Context.User.GetEmbedAuthor("wants to give high fives!"),
            Description = $"{highFiveText}",
            Color = Color.DarkGreen,
        };
    }
}
