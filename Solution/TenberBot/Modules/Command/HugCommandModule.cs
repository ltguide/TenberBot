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
public class HugCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly static Regex RecipientVariables = new(@"%user%|%recipient%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private readonly static Regex SelfVariables = new(@"%user%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private readonly static Regex StatVariables = new(@"%count%|%s%|%es%", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly IHugDataService hugDataService;
    private readonly IVisualDataService visualDataService;
    private readonly IUserStatDataService userStatDataService;
    private readonly ILogger<HugCommandModule> logger;

    public HugCommandModule(
        IHugDataService hugDataService,
        IVisualDataService visualDataService,
        IUserStatDataService userStatDataService,
        ILogger<HugCommandModule> logger)
    {
        this.hugDataService = hugDataService;
        this.visualDataService = visualDataService;
        this.userStatDataService = userStatDataService;
        this.logger = logger;
    }

    [Command("hug")]
    [Summary("Spreads the love.")]
    public async Task Hug([Remainder] string? message = null)
    {
        var visual = await visualDataService.GetRandom(VisualType.Hug);
        if (visual == null)
            return;

        EmbedBuilder embedBuilder;

        var recipient = Context.Message.MentionedUsers.FirstOrDefault();

        if (recipient == null || recipient == Context.User)
        {
            var hug = await hugDataService.GetRandom(HugType.Self);

            if (hug == null)
                return;

            embedBuilder = GetSelfEmbed(hug);
        }
        else
        {
            var hug = await hugDataService.GetRandom(HugType.Recipient);
            var stat = await hugDataService.GetRandom(HugType.Stat);

            if (hug == null || stat == null)
                return;

            ++(await userStatDataService.GetOrAddById(Context)).HugsGiven;

            var received = ++(await userStatDataService.GetOrAddById(Context.Guild.Id, recipient.Id)).HugsReceived;

            await userStatDataService.Save();

            embedBuilder = GetRecipientEmbed(recipient, hug, stat, received);
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

    private EmbedBuilder GetRecipientEmbed(SocketUser recipient, Hug hug, Hug stat, int count)
    {
        var hugText = RecipientVariables.Replace(hug.Text, (match) =>
        {
            return match.Value switch
            {
                "%recipient%" => recipient.GetMention(),
                "%user%" => Context.User.GetMention(),
                _ => match.Value,
            };
        });

        var statText = StatVariables.Replace(stat.Text, (match) =>
        {
            return match.Value switch
            {
                "%count%" => count.ToString("N0"),
                "%s%" => count != 1 ? "s" : "",
                "%es%" => count != 1 ? "es" : "",
                _ => match.Value,
            };
        });

        return new EmbedBuilder
        {
            Author = Context.User.GetEmbedAuthor("is spreading the love!"),
            Description = $"{hugText}\n\n{statText}",
            Color = Color.Green,
        };
    }

    private EmbedBuilder GetSelfEmbed(Hug hug)
    {
        var hugText = SelfVariables.Replace(hug.Text, (match) =>
        {
            return match.Value switch
            {
                "%user%" => Context.User.GetMention(),
                _ => match.Value,
            };
        });

        return new EmbedBuilder
        {
            Author = Context.User.GetEmbedAuthor("wants to spread the love!"),
            Description = $"{hugText}",
            Color = Color.DarkGreen,
        };
    }
}
