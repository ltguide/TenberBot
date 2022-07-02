using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Text.RegularExpressions;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Data.Services;
using TenberBot.Extensions;

namespace TenberBot.Modules.Command;

[RequireUserPermission(ChannelPermission.SendMessages)]
[RequireBotPermission(ChannelPermission.SendMessages)]
public class HugCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly static Regex UserVariables = new(@"%user%|%recipient%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private readonly static Regex SelfVariables = new(@"%user%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private readonly static Regex StatVariables = new(@"%count%|%s%", RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
            var self = await hugDataService.GetRandom(HugType.Self);

            if (self == null)
                return;

            embedBuilder = GetSelfEmbed(self);
        }
        else
        {
            var user = await hugDataService.GetRandom(HugType.User);
            var stat = await hugDataService.GetRandom(HugType.Stat);

            if (user == null || stat == null)
                return;

            ++(await userStatDataService.GetOrAddById(Context)).HugsGiven;

            var received = ++(await userStatDataService.GetOrAddById(Context.Guild.Id, recipient.Id)).HugsReceived;

            await userStatDataService.Save();

            embedBuilder = GetUserEmbed(recipient, user, stat, received);
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

    private EmbedBuilder GetUserEmbed(SocketUser recipient, Hug user, Hug stat, int count)
    {
        var hugText = UserVariables.Replace(user.Text, (match) =>
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

    private EmbedBuilder GetSelfEmbed(Hug self)
    {
        var hugText = SelfVariables.Replace(self.Text, (match) =>
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
