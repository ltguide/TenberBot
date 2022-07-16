using Discord;
using Discord.Interactions;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Data.POCO;
using TenberBot.Data.Services;
using TenberBot.Extensions;

namespace TenberBot.Modules.Interaction;

public class LeaderboardInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private const int PerPage = 15;

    private readonly IUserLevelDataService userLevelDataService;
    private readonly IInteractionParentDataService interactionParentDataService;

    public LeaderboardInteractionModule(
        IUserLevelDataService userLevelDataService,
        IInteractionParentDataService interactionParentDataService)
    {
        this.userLevelDataService = userLevelDataService;
        this.interactionParentDataService = interactionParentDataService;
    }

    [ComponentInteraction("leaderboard:view-*,*")]
    public async Task View(string leaderboardType, ulong messageId)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.Leaderboard, messageId);
        if (parent == null)
            return;

        if (Context.User.Id != parent.UserId)
        {
            await RespondAsync("Sorry, you can't interact with this message.", ephemeral: true);
            return;
        }

        var view = parent.GetReference<LeaderboardView>()!;

        view.PageNumber = 0;
        view.PageCount = await userLevelDataService.GetCount(Context.Guild.Id, PerPage);
        view.LeaderboardType = Enum.Parse<LeaderboardType>(leaderboardType, true);

        parent.SetReference(view);

        await interactionParentDataService.Update(parent, null!);

        await DeferAsync();

        var embed = await GetEmbed(view);

        await ModifyOriginalResponseAsync(x =>
        {
            x.Components = GetButtons(messageId, view.LeaderboardType);
            x.Content = null;
            x.Embed = embed;
        });
    }

    [ComponentInteraction("leaderboard:page-*,*")]
    public async Task Page(string page, ulong messageId)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.Leaderboard, messageId);
        if (parent == null)
            return;

        if (Context.User.Id != parent.UserId)
        {
            await RespondAsync("Sorry, you can't interact with this message.", ephemeral: true);
            return;
        }

        var pageCount = await userLevelDataService.GetCount(Context.Guild.Id, PerPage);

        var view = parent.GetReference<LeaderboardView>()!;

        view.PageCount = await userLevelDataService.GetCount(Context.Guild.Id, PerPage);
        view.PageNumber = await GetPageNumber(page, parent, view);

        parent.SetReference(view);

        await interactionParentDataService.Update(parent, null!);

        await DeferAsync();

        var embed = await GetEmbed(view);

        await ModifyOriginalResponseAsync(x => x.Embed = embed);
    }

    private async Task<int> GetPageNumber(string page, InteractionParent parent, LeaderboardView view)
    {
        if (page == "user")
        {
            var userLevel = await userLevelDataService.GetByIds(parent.GuildId, parent.UserId!.Value);
            if (userLevel == null)
                return 0;

            await userLevelDataService.GetRanks(userLevel);

            return (int)Math.Floor((decimal)(view.LeaderboardType == LeaderboardType.Message ? userLevel.MessageRank : userLevel.VoiceRank) / PerPage);
        }

        return page switch
        {
            "first" => 0,
            "previous" => Math.Max(0, view.PageNumber - 1),
            "next" => Math.Min(view.PageCount, view.PageNumber + 1),
            "last" => view.PageCount,
            _ => throw new NotImplementedException(),
        };
    }

    private static MessageComponent GetButtons(ulong messageId, LeaderboardType leaderboardType)
    {
        var componentBuilder = new ComponentBuilder()
            .WithButton(customId: $"leaderboard:page-first,{messageId}", emote: new Emoji("⏮"))
            .WithButton(customId: $"leaderboard:page-previous,{messageId}", emote: new Emoji("⏪"))
            .WithButton(customId: $"leaderboard:page-user,{messageId}", emote: new Emoji("🪞"))
            .WithButton(customId: $"leaderboard:page-next,{messageId}", emote: new Emoji("⏩"))
            .WithButton(customId: $"leaderboard:page-last,{messageId}", emote: new Emoji("⏭"));

        if (leaderboardType == LeaderboardType.Message)
            componentBuilder.WithButton("Switch to Voice", $"leaderboard:view-voice,{messageId}", ButtonStyle.Secondary, new Emoji("🎤"), row: 1);

        if (leaderboardType == LeaderboardType.Voice)
            componentBuilder.WithButton("Switch to Message", $"leaderboard:view-message,{messageId}", ButtonStyle.Secondary, new Emoji("📝"), row: 1);

        return componentBuilder.Build();
    }

    private async Task<Embed> GetEmbed(LeaderboardView view)
    {
        var userLevels = await userLevelDataService.GetPage(Context.Guild.Id, PerPage, view.PageNumber, view.LeaderboardType);

        var embedBuilder = new EmbedBuilder
        {
            Title = $"Viewing {view.LeaderboardType} Leaderboard",
        }.WithFooter($"{view.PageNumber + 1} of {view.PageCount + 1}");

        if (userLevels.Count == 0)
        {
            embedBuilder.WithDescription("no results");
            return embedBuilder.Build();
        }

        Func<UserLevel, int, string> wtf;

        if (view.LeaderboardType == LeaderboardType.Message)
            wtf = (x, a) => $"`#{a + 1 + (PerPage * view.PageNumber),-4}` {x.UserId.GetUserMention()} (level {x.MessageLevel,4}) has {x.MessageExperience:N2} exp";

        else
            wtf = (x, a) => $"`#{a + 1 + (PerPage * view.PageNumber),-4}` {x.UserId.GetUserMention()} (level {x.VoiceLevel,4}) has {x.VoiceExperience:N2} exp";

        embedBuilder.WithDescription(string.Join("\n", userLevels.Select(wtf)));

        return embedBuilder.Build();
    }
}
