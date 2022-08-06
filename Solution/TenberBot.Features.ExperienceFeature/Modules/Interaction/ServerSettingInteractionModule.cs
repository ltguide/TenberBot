using Discord;
using Discord.Interactions;
using TenberBot.Features.ExperienceFeature.Data.Models;
using TenberBot.Features.ExperienceFeature.Data.Services;
using TenberBot.Features.ExperienceFeature.Helpers;
using TenberBot.Features.ExperienceFeature.Settings.Server;
using TenberBot.Shared.Features.Data.Models;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.Caches;
using TenberBot.Shared.Features.Extensions.DiscordRoot;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;
using TenberBot.Shared.Features.Services;
using Color = SixLabors.ImageSharp.Color;

namespace TenberBot.Features.ExperienceFeature.Modules.Interaction;

[Group("server-experience", "Manage server settings for the bot.")]
[DefaultMemberPermissions(GuildPermission.ManageGuild)]
public class ServerExperienceInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IRankCardDataService rankCardDataService;
    private readonly IServerSettingDataService serverSettingDataService;
    private readonly WebService webService;
    private readonly CacheService cacheService;

    public ServerExperienceInteractionModule(
        IRankCardDataService rankCardDataService,
        IServerSettingDataService serverSettingDataService,
        WebService webService,
        CacheService cacheService)
    {
        this.rankCardDataService = rankCardDataService;
        this.serverSettingDataService = serverSettingDataService;
        this.webService = webService;
        this.cacheService = cacheService;
    }

    [SlashCommand("rank-card", "Configure the rank cards.")]
    public async Task Rank(
        IRole role,
        [Summary("image")] IAttachment? image = null,
        [Summary("colors")] string? colors = null,
        [Summary("guild-color")] string? guildColor = null,
        [Summary("user-color")] string? userColor = null,
        [Summary("role-color")] string? roleColor = null,
        [Summary("rank-color")] string? rankColor = null,
        [Summary("level-color")] string? levelColor = null,
        [Summary("experience-color")] string? experienceColor = null,
        [Summary("progress-color")] string? progressColor = null,
        [Summary("progress-fill")] string? progressFill = null)
    {
        var card = await rankCardDataService.GetByRoleId(role.Id);
        if (card == null)
            await rankCardDataService.Add(card = new RankCard { GuildId = Context.Guild.Id, RoleId = role.Id, });

        if (image != null)
        {
            var file = await webService.GetFileAttachment(image.Url);
            if (file != null)
            {
                card.Data = file.Value.GetBytes();
                card.Filename = image.Filename;
            }
        }

        if (colors != null && Color.TryParseHex(colors, out var color))
        {
            var hex = color.ToHex();

            card.GuildColor = hex;
            card.UserColor = hex;
            card.RoleColor = hex;
            card.RankColor = hex;
            card.LevelColor = hex;
            card.ExperienceColor = hex;
            card.ProgressColor = hex;
        }

        if (guildColor != null && Color.TryParseHex(guildColor, out color))
            card.GuildColor = color.ToHex();

        if (userColor != null && Color.TryParseHex(userColor, out color))
            card.UserColor = color.ToHex();

        if (roleColor != null && Color.TryParseHex(roleColor, out color))
            card.RoleColor = color.ToHex();

        if (rankColor != null && Color.TryParseHex(rankColor, out color))
            card.RankColor = color.ToHex();

        if (levelColor != null && Color.TryParseHex(levelColor, out color))
            card.LevelColor = color.ToHex();

        if (experienceColor != null && Color.TryParseHex(experienceColor, out color))
            card.ExperienceColor = color.ToHex();

        if (progressColor != null && Color.TryParseHex(progressColor, out color))
            card.ProgressColor = color.ToHex();

        if (progressFill != null && Color.TryParseHex(progressFill, out color))
            card.ProgressFill = color.ToHex();

        await rankCardDataService.Update(card, null!);

        var text = $"Server settings for *rank-card* for {role.Mention}:\n\n> **Guild Color**: #{card.GuildColor}\n> **User Color**: #{card.UserColor}\n> **Role Color**: #{card.RoleColor}\n> **Rank Color**: #{card.RankColor}\n> **Level Color**: #{card.LevelColor}\n> **Experience Color**: #{card.ExperienceColor}\n\n> **Progress Color**: #{card.ProgressColor}\n> **Progress Fill**: #{card.ProgressFill}\n\n> **Background Image**: ";

        if (card.Data != null && card.Data.Length != 0)
        {
            card.Name = "Role Name";

            var userLevel = new UserLevel()
            {
                MessageExperience = 987654321.99m,
                MessageRank = 999,
                VoiceExperience = 123400789.55m,
                VoiceRank = 555,
            };

            userLevel.UpdateVoiceLevel();
            userLevel.UpdateMessageLevel();

            var myAvatar = await webService.GetBytes(Context.Client.CurrentUser.GetCurrentAvatarUrl(), TimeSpan.FromMinutes(60));
            var userAvatar = await webService.GetBytes(Context.User.GetCurrentAvatarUrl(), TimeSpan.FromMinutes(5));

            using var memoryStream = RankCardHelper.GetStream(card, Context.Guild, Context.User, userLevel, myAvatar, userAvatar);

            await Context.Interaction.RespondWithFileAsync(new FileAttachment(memoryStream, $"{Context.User.Id}_{card.Filename}"), text);
        }
        else
            await RespondAsync(text + "*none*");
    }

    [SlashCommand("leaderboard", "Configure the leaderboard.")]
    public async Task Leaderboard(
        [Summary("display-event")] bool? eventEnabled = null)
    {
        var settings = cacheService.Get<LeaderboardServerSettings>(Context.Guild);

        if (eventEnabled != null)
            settings.DisplayEvent = eventEnabled.Value;

        await Set(settings);

        await RespondAsync($"Server setting:\n\n> **display-event**: {settings.DisplayEvent}");
    }

    private async Task Set<T>(T value)
    {
        var key = CacheService.GetSettingsKey<T>();

        cacheService.Cache.Set(Context.Guild, key, value);

        var setting = await serverSettingDataService.GetByName(Context.Guild.Id, key);
        if (setting == null)
            await serverSettingDataService.Add(new ServerSetting { GuildId = Context.Guild.Id, Name = key, }.SetValue(value));
        else
            await serverSettingDataService.Update(setting, new ServerSetting().SetValue(value));
    }
}
