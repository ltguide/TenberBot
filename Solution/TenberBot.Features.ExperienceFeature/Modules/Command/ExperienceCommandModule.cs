﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TenberBot.Features.ExperienceFeature.Data.InteractionParents;
using TenberBot.Features.ExperienceFeature.Data.Models;
using TenberBot.Features.ExperienceFeature.Data.POCO;
using TenberBot.Features.ExperienceFeature.Data.Services;
using TenberBot.Features.ExperienceFeature.Helpers;
using TenberBot.Features.ExperienceFeature.Settings.Server;
using TenberBot.Shared.Features.Data.Models;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;
using TenberBot.Shared.Features.Results.Command;
using TenberBot.Shared.Features.Services;
using TenberBot.Shared.Features.Settings.Server;

namespace TenberBot.Features.ExperienceFeature.Modules.Command;

[Remarks("Information")]
public class ExperienceCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly IInteractionParentDataService interactionParentDataService;
    private readonly IUserLevelDataService userLevelDataService;
    private readonly VisualWebService visualWebService;
    private readonly IRankCardDataService rankCardDataService;
    private readonly CacheService cacheService;

    public ExperienceCommandModule(
        IInteractionParentDataService interactionParentDataService,
        IUserLevelDataService userLevelDataService,
        VisualWebService visualWebService,
        IRankCardDataService rankCardDataService,
        CacheService cacheService)
    {
        this.interactionParentDataService = interactionParentDataService;
        this.userLevelDataService = userLevelDataService;
        this.visualWebService = visualWebService;
        this.rankCardDataService = rankCardDataService;
        this.cacheService = cacheService;
    }

    [Command("rank", ignoreExtraArgs: true)]
    [Alias("lvl", "level")]
    [Summary("See your experience information.")]
    public async Task<RuntimeResult> Rank()
    {
        await Context.Message.AddReactionAsync(cacheService.Get<EmoteServerSettings>(Context.Guild).Busy);

        var userLevel = await userLevelDataService.GetByContext(Context);
        if (userLevel == null)
            return DeleteResult.FromError("You have no experience right now.");

        var card = await FindCard();
        if (card == null)
            return DeleteResult.FromError("Unable to find a configured rank card.");

        var myAvatar = await visualWebService.GetBytes(Context.Client.CurrentUser.GetCurrentAvatarUrl(), TimeSpan.FromMinutes(60));
        if (myAvatar == null)
            return DeleteResult.FromError("Failed to load my avatar. Please try again.");

        var userAvatar = await visualWebService.GetBytes(Context.User.GetCurrentAvatarUrl(), TimeSpan.FromMinutes(5));
        if (userAvatar == null)
            return DeleteResult.FromError("Failed to load your avatar. Please try again.");

        await userLevelDataService.LoadRanks(userLevel);

        using var memoryStream = RankCardHelper.GetStream(card, Context.Guild, Context.User, userLevel, myAvatar, userAvatar);

        await Context.Channel.SendFileAsync(new FileAttachment(memoryStream, $"{Context.User.Id}_{card.Filename}"), messageReference: Context.Message.GetReferenceTo());

        await Context.Message.RemoveAllReactionsAsync();

        return DeleteResult.FromSuccess("");
    }

    [Command("leaderboard", ignoreExtraArgs: true)]
    [Alias("lb")]
    [Summary("View experience leaderboard.")]
    public async Task Leaderboard()
    {
        await Context.Message.AddReactionAsync(cacheService.Get<EmoteServerSettings>(Context.Guild).Success);

        var reply = await Context.Message.ReplyAsync("Which leaderboard do you want to view?");

        var parent = await interactionParentDataService.Set(new InteractionParent
        {
            GuildId = Context.Guild.Id,
            ChannelId = Context.Channel.Id,
            UserId = Context.User.Id,
            InteractionParentType = InteractionParents.Leaderboard,
            MessageId = reply.Id,
        }.SetReference(new LeaderboardView()));

        try
        {
            if (parent != null)
                await Context.Channel.DeleteMessageAsync(parent.Value);
        }
        catch (Exception) { }

        var components = new ComponentBuilder()
            .WithButton("Message", $"leaderboard:view-message,{reply.Id}", emote: new Emoji("📝"))
            .WithButton("Voice", $"leaderboard:view-voice,{reply.Id}", emote: new Emoji("🎤"));

        var settings = cacheService.Get<LeaderboardServerSettings>(Context.Guild);
        if (settings.DisplayEventA)
            components.WithButton("Event A", $"leaderboard:view-eventa,{reply.Id}", emote: new Emoji("🎟"));

        if (settings.DisplayEventB)
            components.WithButton("Event B", $"leaderboard:view-eventb,{reply.Id}", emote: new Emoji("🎫"));

        await reply.ModifyAsync(x => x.Components = components.Build());
    }

    private async Task<RankCard?> FindCard()
    {
        if (Context.User is not SocketGuildUser user)
            return null;

        var cards = (await rankCardDataService.GetAllByGuildId(Context.Guild.Id)).Where(x => x.Data != null && x.Data.Length != 0).ToDictionary(x => x.RoleId, x => x);
        if (cards.Count == 0)
            return null;

        foreach (var role in user.Roles.OrderByDescending(x => x.Position))
        {
            if (cards.TryGetValue(role.Id, out var card))
            {
                card.Name = role.Name;
                return card;
            }
        }

        return cards.First().Value;
    }
}
