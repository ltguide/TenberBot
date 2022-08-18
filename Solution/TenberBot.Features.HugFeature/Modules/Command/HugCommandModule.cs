using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Text.RegularExpressions;
using TenberBot.Features.HugFeature.Data.Enums;
using TenberBot.Features.HugFeature.Data.Models;
using TenberBot.Features.HugFeature.Data.Services;
using TenberBot.Shared.Features.Data.Enums;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.DiscordCommands;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;

namespace TenberBot.Features.HugFeature.Modules.Command;

[RequireBotPermission(ChannelPermission.SendMessages)]
public class HugCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly static Regex RecipientVariables = new(@"%user%|%recipient%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private readonly static Regex SelfVariables = new(@"%user%|%random%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private readonly static Regex StatVariables = new(@"%user%|%recipient%|%count%|%s%|%es%", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly IHugDataService hugDataService;
    private readonly IVisualDataService visualDataService;
    private readonly IUserStatDataService userStatDataService;

    public HugCommandModule(
        IHugDataService hugDataService,
        IVisualDataService visualDataService,
        IUserStatDataService userStatDataService)
    {
        this.hugDataService = hugDataService;
        this.visualDataService = visualDataService;
        this.userStatDataService = userStatDataService;
    }

    [Command("hug")]
    [Summary("Spreads the love.\n*If you reply to a user, `<user>` is no longer required.*")]
    [Remarks("`<user>` `[message]`")]
    public async Task Hug([Remainder] string? message = null)
    {
        var recipient = Context.Message.MentionedUsers.FirstOrDefault();
        var hugType = (recipient == null || recipient == Context.User) ? VisualType.HugSelf : VisualType.Hug;

        var visual = await visualDataService.GetRandom(hugType);
        if (visual == null)
            return;

        EmbedBuilder embedBuilder;

        if (hugType == VisualType.HugSelf)
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

            ++(await userStatDataService.GetOrAddByContext(Context)).HugsGiven;

            var received = ++(await userStatDataService.GetOrAddByIds(Context.Guild.Id, recipient!.Id)).HugsReceived;

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
        var primaryText = RecipientVariables.Replace(hug.Text, (match) =>
        {
            return match.Value.ToLower() switch
            {
                "%user%" => Context.User.GetMention(),
                "%recipient%" => recipient.GetMention(),
                _ => match.Value,
            };
        });

        var statText = StatVariables.Replace(stat.Text, (match) =>
        {
            return match.Value.ToLower() switch
            {
                "%user%" => Context.User.GetMention(),
                "%recipient%" => recipient.GetMention(),
                "%count%" => count.ToString("N0"),
                "%s%" => count != 1 ? "s" : "",
                "%es%" => count != 1 ? "es" : "",
                _ => match.Value,
            };
        });

        return new EmbedBuilder
        {
            Author = Context.User.GetEmbedAuthor("is spreading the love!"),
            Description = $"{primaryText}\n\n{statText}",
            Color = Color.Green,
        };
    }

    private EmbedBuilder GetSelfEmbed(Hug hug)
    {
        var hugText = SelfVariables.Replace(hug.Text, (match) =>
        {
            return match.Value.ToLower() switch
            {
                "%user%" => Context.User.GetMention(),
                "%random%" => Context.GetRandomUser()?.GetDisplayNameSanitized() ?? "Random User",
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
