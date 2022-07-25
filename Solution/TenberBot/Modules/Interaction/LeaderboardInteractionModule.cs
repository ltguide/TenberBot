using Discord;
using Discord.Interactions;
using TenberBot.Data.Enums;
using TenberBot.Data.POCO;
using TenberBot.Data.Services;
using TenberBot.Extensions;

namespace TenberBot.Modules.Interaction;

public class LeaderboardInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
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

        view.LeaderboardType = Enum.Parse<LeaderboardType>(leaderboardType, true);
        view.MinimumExperience = view.CalcMinimumExperience();
        view.UserPage = await userLevelDataService.GetUserPage(parent.GuildId, parent.UserId.Value, view);
        view.CurrentPage = 0;
        view.PageCount = await userLevelDataService.GetCount(Context.Guild.Id, view);

        parent.SetReference(view);

        await interactionParentDataService.Update(parent, null!);

        await DeferAsync();

        var embed = await GetEmbed(view);

        await ModifyOriginalResponseAsync(x =>
        {
            x.Components = GetButtons(messageId, view.LeaderboardType);
            x.Content = GetContent(view);
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

        var view = parent.GetReference<LeaderboardView>()!;

        view.UserPage = await userLevelDataService.GetUserPage(parent.GuildId, parent.UserId.Value, view);
        view.PageCount = await userLevelDataService.GetCount(Context.Guild.Id, view);
        view.CurrentPage = view.GetNewPage(page);

        parent.SetReference(view);

        await interactionParentDataService.Update(parent, null!);

        await DeferAsync();

        var embed = await GetEmbed(view);

        await ModifyOriginalResponseAsync(x =>
        {
            x.Content = GetContent(view);
            x.Embed = embed;
        });
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

        componentBuilder.WithButton(customId: $"leaderboard:page-refresh,{messageId}", style: ButtonStyle.Secondary, emote: new Emoji("🔁"), row: 1);

        return componentBuilder.Build();
    }

    private string GetContent(LeaderboardView view)
    {
        if (view.UserPage == -1)
            return $"Sorry, {Context.User.Id.GetUserMention()}, you need at least {view.MinimumExperience:N0} experience to show up on the {view.LeaderboardType} Leaderboard. I'd love to hear more about you! 💖";

        var flavor = view.UserPage == 0 ? "Congrats! ✨" : "You're doing awesome! 💖";
        var pin = view.CurrentPage != view.UserPage ? "\nYou can click the 🪞 button to jump to your page." : "";

        return $"Hey, {Context.User.Id.GetUserMention()}, you are on **page {view.UserPage + 1}** of the {view.LeaderboardType} Leaderboard. {flavor}{pin}";
    }

    private async Task<Embed> GetEmbed(LeaderboardView view)
    {
        var userLevels = await userLevelDataService.GetPage(Context.Guild.Id, view);

        var embedBuilder = new EmbedBuilder
        {
            Author = Context.User.GetEmbedAuthor($"is viewing the {view.LeaderboardType} Leaderboard"),
        }
        .WithFooter($"Page {view.CurrentPage + 1} of {view.PageCount + 1}")
        .WithCurrentTimestamp();

        if (userLevels.Count == 0)
        {
            embedBuilder.WithDescription("No results found.");
            return embedBuilder.Build();
        }

        var lines = userLevels.Select((x, index) =>
        {
            var (level, experience) = x.GetLeaderboardData(view.LeaderboardType);

            var name = x.UserId == Context.User.Id ? x.UserId.GetUserMention() : $"`{x.ServerUser.DisplayName}`";

            return $"`#{index + view.BaseRank,-4}` {name} has {experience:N2} XP (lvl **{level}**)";
        });

        embedBuilder.WithDescription(string.Join("\n", lines));

        return embedBuilder.Build();
    }
}
