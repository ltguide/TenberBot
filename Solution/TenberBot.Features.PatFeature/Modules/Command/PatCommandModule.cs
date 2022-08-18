using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Text.RegularExpressions;
using TenberBot.Features.PatFeature.Data.Enums;
using TenberBot.Features.PatFeature.Data.Models;
using TenberBot.Features.PatFeature.Data.Services;
using TenberBot.Shared.Features.Data.Enums;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.DiscordCommands;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;

namespace TenberBot.Features.PatFeature.Modules.Command;

[RequireBotPermission(ChannelPermission.SendMessages)]
public class PatCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly static Regex RecipientVariables = new(@"%user%|%recipient%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private readonly static Regex SelfVariables = new(@"%user%|%random%", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private readonly static Regex StatVariables = new(@"%user%|%recipient%|%count%|%s%|%es%", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly IPatDataService patDataService;
    private readonly IVisualDataService visualDataService;
    private readonly IUserStatDataService userStatDataService;

    public PatCommandModule(
        IPatDataService patDataService,
        IVisualDataService visualDataService,
        IUserStatDataService userStatDataService)
    {
        this.patDataService = patDataService;
        this.visualDataService = visualDataService;
        this.userStatDataService = userStatDataService;
    }

    [Command("pat")]
    [Alias("headpat")]
    [Summary("Spreads the love.\n*If you reply to a user, `<user>` is no longer required.*")]
    [Remarks("`<user>` `[message]`")]
    public async Task Pat([Remainder] string? message = null)
    {
        var recipient = Context.Message.MentionedUsers.FirstOrDefault();
        var patType = (recipient == null || recipient == Context.User) ? VisualType.PatSelf : VisualType.Pat;

        var visual = await visualDataService.GetRandom(patType);
        if (visual == null)
            return;

        EmbedBuilder embedBuilder;

        if (patType == VisualType.PatSelf)
        {
            var pat = await patDataService.GetRandom(PatType.Self);

            if (pat == null)
                return;

            embedBuilder = GetSelfEmbed(pat);
        }
        else
        {
            var pat = await patDataService.GetRandom(PatType.Recipient);
            var stat = await patDataService.GetRandom(PatType.Stat);

            if (pat == null || stat == null)
                return;

            ++(await userStatDataService.GetOrAddByContext(Context)).PatsGiven;

            var received = ++(await userStatDataService.GetOrAddByIds(Context.Guild.Id, recipient!.Id)).PatsReceived;

            await userStatDataService.Save();

            embedBuilder = GetRecipientEmbed(recipient, pat, stat, received);
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

    private EmbedBuilder GetRecipientEmbed(SocketUser recipient, Pat pat, Pat stat, int count)
    {
        var primaryText = RecipientVariables.Replace(pat.Text, (match) =>
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

    private EmbedBuilder GetSelfEmbed(Pat pat)
    {
        var patText = SelfVariables.Replace(pat.Text, (match) =>
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
            Description = $"{patText}",
            Color = Color.DarkGreen,
        };
    }
}
