using Discord;
using Discord.Interactions;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Data.Services;
using TenberBot.Data.Settings.Server;
using TenberBot.Extensions;
using TenberBot.Helpers;
using TenberBot.Services;
using Color = SixLabors.ImageSharp.Color;

namespace TenberBot.Modules.Interaction;

[Group("server-setting", "Manage server settings for the bot.")]
[DefaultMemberPermissions(GuildPermission.ManageGuild)]
public class ServerSettingInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IRankCardDataService rankCardDataService;
    private readonly IServerSettingDataService serverSettingDataService;
    private readonly WebService webService;
    private readonly CacheService cacheService;

    public ServerSettingInteractionModule(
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

    [SlashCommand("prefix", "Configure the prefix for message commands.")]
    public async Task Prefix(string? value = null)
    {
        var settings = cacheService.Get<BasicServerSettings>(Context.Guild);

        if (value != null)
            settings.Prefix = value == "none" ? "" : value;

        await Set(settings);

        if (settings.Prefix == "")
            await RespondAsync($"Server setting:\n\n> **Prefix**: *none*\n\nI am not able to respond to chat messages.");
        else
            await RespondAsync($"Server setting:\n\n> **Prefix**: {settings.Prefix}");
    }

    [SlashCommand("emote", "Configure the reaction emotes.")]
    public async Task Emote(
        string? success = null,
        string? fail = null,
        string? busy = null)
    {
        var settings = cacheService.Get<EmoteServerSettings>(Context.Guild);

        if (success != null)
            settings.Success = success.AsIEmote() ?? new EmoteServerSettings().Success;

        if (fail != null)
            settings.Fail = fail.AsIEmote() ?? new EmoteServerSettings().Fail;

        if (busy != null)
            settings.Busy = busy.AsIEmote() ?? new EmoteServerSettings().Busy;

        await Set(settings);

        await RespondAsync($"Server settings for *emote*:\n\n> **{SetEmoteChoice.Success}**: {settings.Success}\n> **{SetEmoteChoice.Fail}**: {settings.Fail}\n> **{SetEmoteChoice.Busy}**: {settings.Busy}");
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

        if (card.Data != null)
        {
            card.Name = "Role Name";

            var userLevel = new UserLevel()
            {
                MessageExperience = 987654321.99m,
                MessageRank = 999,
                VoiceExperience = 123456789.55m,
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

    private async Task Set<T>(T value)
    {
        var key = cacheService.GetSettingsKey<T>();

        cacheService.Cache.Set(Context.Guild, key, value);

        var setting = await serverSettingDataService.GetByName(Context.Guild.Id, key);
        if (setting == null)
            await serverSettingDataService.Add(new ServerSetting { GuildId = Context.Guild.Id, Name = key, }.SetValue(value));
        else
            await serverSettingDataService.Update(setting, new ServerSetting().SetValue(value));
    }
}
