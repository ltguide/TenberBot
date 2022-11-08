using Discord;
using Discord.Interactions;
using TenberBot.Features.ExperienceFeature.Data.Enums;
using TenberBot.Features.ExperienceFeature.Data.POCO;
using TenberBot.Features.ExperienceFeature.Data.Services;
using TenberBot.Features.ExperienceFeature.Settings.Server;
using TenberBot.Shared.Features.Data.Enums;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;
using TenberBot.Shared.Features.Extensions.Mentions;
using TenberBot.Shared.Features.Services;

namespace TenberBot.Features.ExperienceFeature.Modules.Interaction;

[EnabledInDm(false)]
public class LeaderboardInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IUserLevelDataService userLevelDataService;
    private readonly IInteractionParentDataService interactionParentDataService;
    private readonly CacheService cacheService;

    public LeaderboardInteractionModule(
        IUserLevelDataService userLevelDataService,
        IInteractionParentDataService interactionParentDataService,
        CacheService cacheService)
    {
        this.userLevelDataService = userLevelDataService;
        this.interactionParentDataService = interactionParentDataService;
        this.cacheService = cacheService;
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

        if ((view.LeaderboardType == LeaderboardType.EventA && cacheService.Get<LeaderboardServerSettings>(Context.Guild).DisplayEventA == false)
            || (view.LeaderboardType == LeaderboardType.EventB && cacheService.Get<LeaderboardServerSettings>(Context.Guild).DisplayEventB == false))
            view.LeaderboardType = LeaderboardType.Message;


        view.MinimumExperience = view.CalcMinimumExperience();
        view.CurrentPage = 0;
        view.UserPage = await userLevelDataService.GetUserPage(parent.GuildId, parent.UserId.Value, view);
        view.PageCount = await userLevelDataService.GetCount(Context.Guild.Id, view);

        parent.SetReference(view);

        await interactionParentDataService.Update(parent, null!);

        await DeferAsync();

        var embed = await GetEmbed(view);

        await ModifyOriginalResponseAsync(x =>
        {
            x.Components = GetComponents(messageId, view.LeaderboardType);
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

        if ((view.LeaderboardType == LeaderboardType.EventA && cacheService.Get<LeaderboardServerSettings>(Context.Guild).DisplayEventA == false)
            || (view.LeaderboardType == LeaderboardType.EventB && cacheService.Get<LeaderboardServerSettings>(Context.Guild).DisplayEventB == false))
        {
            await View(LeaderboardType.Message.ToString(), messageId);
            return;
        }

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

    private MessageComponent GetComponents(ulong messageId, LeaderboardType leaderboardType)
    {
        var componentBuilder = new ComponentBuilder()
            .WithButton(customId: $"leaderboard:page-first,{messageId}", emote: new Emoji("⏮"))
            .WithButton(customId: $"leaderboard:page-previous,{messageId}", emote: new Emoji("⏪"))
            .WithButton(customId: $"leaderboard:page-user,{messageId}", emote: new Emoji("🪞"))
            .WithButton(customId: $"leaderboard:page-next,{messageId}", emote: new Emoji("⏩"))
            .WithButton(customId: $"leaderboard:page-last,{messageId}", emote: new Emoji("⏭"));

        var viewButtons = leaderboardType switch
        {
            LeaderboardType.Message => ViewButtons.Voice | ViewButtons.EventA | ViewButtons.EventB,
            LeaderboardType.Voice => ViewButtons.Message | ViewButtons.EventA | ViewButtons.EventB,
            LeaderboardType.EventA => ViewButtons.Message | ViewButtons.Voice | ViewButtons.EventB,
            LeaderboardType.EventB => ViewButtons.Message | ViewButtons.Voice | ViewButtons.EventA,
            _ => ViewButtons.None,
        };

        if (viewButtons.HasFlag(ViewButtons.Message))
            componentBuilder.WithButton("Message", $"leaderboard:view-message,{messageId}", ButtonStyle.Secondary, new Emoji("📝"), row: 1);

        if (viewButtons.HasFlag(ViewButtons.Voice))
            componentBuilder.WithButton("Voice", $"leaderboard:view-voice,{messageId}", ButtonStyle.Secondary, new Emoji("🎤"), row: 1);

        if (viewButtons.HasFlag(ViewButtons.EventA) && cacheService.Get<LeaderboardServerSettings>(Context.Guild).DisplayEventA)
            componentBuilder.WithButton("Event A", $"leaderboard:view-eventa,{messageId}", ButtonStyle.Secondary, new Emoji("🎟"));

        if (viewButtons.HasFlag(ViewButtons.EventB) && cacheService.Get<LeaderboardServerSettings>(Context.Guild).DisplayEventB)
            componentBuilder.WithButton("Event B", $"leaderboard:view-eventb,{messageId}", ButtonStyle.Secondary, new Emoji("🎫"));

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
            embedBuilder
                .WithDescription("No results found.")
                .WithFooter("");
            return embedBuilder.Build();
        }

        var lines = userLevels.Select((x, index) =>
        {
            var (level, experience) = x.GetLeaderboardData(view.LeaderboardType);

            var name = x.UserId == Context.User.Id ? x.UserId.GetUserMention() : $"`{x.ServerUser.DisplayName}`";

            var levelText = level != -1 ? $" (lvl **{level}**)" : "";

            return $"`#{index + view.BaseRank,-4}` {name} has {experience:N2} XP{levelText}";
        });

        embedBuilder.WithDescription(string.Join("\n", lines));

        return embedBuilder.Build();
    }

    [Flags]
    private enum ViewButtons
    {
        None = 0,
        Message = 1 << 0,
        Voice = 1 << 1,
        EventA = 1 << 2,
        EventB = 1 << 3,
    }
}
