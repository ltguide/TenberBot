using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Text.RegularExpressions;
using TenberBot.Features.HighFiveFeature.Data.Enums;
using TenberBot.Features.HighFiveFeature.Data.Models;
using TenberBot.Features.HighFiveFeature.Data.Services;
using TenberBot.Features.HighFiveFeature.Data.UserStats;
using TenberBot.Features.HighFiveFeature.Data.Visuals;
using TenberBot.Shared.Features.Data.Ids;
using TenberBot.Shared.Features.Data.POCO;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.DiscordCommands;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;

namespace TenberBot.Features.HighFiveFeature.Modules.Command;

[RequireBotPermission(ChannelPermission.SendMessages)]
public partial class HighFiveCommandModule : ModuleBase<SocketCommandContext>
{
    [GeneratedRegex("%user%|%recipient%", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex RecipientVariables();

    [GeneratedRegex("%user%|%random%", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex SelfVariables();

    [GeneratedRegex("%user%|%recipient%|%count%|%s%|%es%", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex StatVariables();

    private readonly IHighFiveDataService highFiveDataService;
    private readonly IVisualDataService visualDataService;
    private readonly IUserStatDataService userStatDataService;

    public HighFiveCommandModule(
        IHighFiveDataService highFiveDataService,
        IVisualDataService visualDataService,
        IUserStatDataService userStatDataService)
    {
        this.highFiveDataService = highFiveDataService;
        this.visualDataService = visualDataService;
        this.userStatDataService = userStatDataService;
    }

    [Command("high-five")]
    [Alias("highfive", "high5", "hi5")]
    [Summary("Up high!\n*If you reply to a user, `<user>` is no longer required.*")]
    [Remarks("`<user>` `[message]`")]
    public async Task HighFive([Remainder] string? message = null)
    {
        var recipient = Context.Message.MentionedUsers.FirstOrDefault();
        var highFiveType = (recipient == null || recipient == Context.User) ? Visuals.Self : Visuals.Recipient;

        var visual = await visualDataService.GetRandom(highFiveType);
        if (visual == null)
            return;

        EmbedBuilder embedBuilder;

        if (highFiveType == Visuals.Self)
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

            var userStats = await userStatDataService.Update(new[] { new UserStatMod(new GuildUserIds(Context), UserStats.Given), new UserStatMod(new GuildUserIds(Context.Guild, recipient!), UserStats.Received) });

            embedBuilder = GetRecipientEmbed(recipient!, highFive, stat, userStats[UserStats.Received].Value);
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
        var primaryText = RecipientVariables().Replace(highFive.Text, (match) =>
        {
            return match.Value.ToLower() switch
            {
                "%user%" => Context.User.GetMention(),
                "%recipient%" => recipient.GetMention(),
                _ => match.Value,
            };
        });

        var statText = StatVariables().Replace(stat.Text, (match) =>
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
            Author = Context.User.GetEmbedAuthor("is giving out high fives!"),
            Description = $"{primaryText}\n\n{statText}",
            Color = Color.Green,
        };
    }

    private EmbedBuilder GetSelfEmbed(HighFive highFive)
    {
        var highFiveText = SelfVariables().Replace(highFive.Text, (match) =>
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
            Author = Context.User.GetEmbedAuthor("wants to give high fives!"),
            Description = $"{highFiveText}",
            Color = Color.DarkGreen,
        };
    }
}
