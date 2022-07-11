using Discord;
using Discord.Interactions;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Data.Services;
using TenberBot.Data.Settings.Server;
using TenberBot.Extensions;
using TenberBot.Services;
using Color = SixLabors.ImageSharp.Color;

namespace TenberBot.Modules.Interaction;

[Group("server-setting", "Manage server settings for the bot.")]
[RequireUserPermission(GuildPermission.ManageGuild)]
public class ServerSettingInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IServerSettingDataService serverSettingDataService;
    private readonly WebService webService;
    private readonly CacheService cacheService;

    public ServerSettingInteractionModule(
        IServerSettingDataService serverSettingDataService,
        WebService webService,
        CacheService cacheService)
    {
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

    [SlashCommand("rank", "Configure the rank system.")]
    public async Task Rank(
        [Summary("background-image")] IAttachment? backgroundImage = null,
        [Summary("background-fill")] string? backgroundFill = null)
    {
        var settings = cacheService.Get<RankServerSettings>(Context.Guild);

        if (backgroundImage != null)
        {
            var file = await webService.GetFileAttachment(backgroundImage.Url);
            if (file != null)
            {
                settings.BackgroundData = file.Value.GetBytes();
                settings.BackgroundName = backgroundImage.Filename;
            }
        }

        if (backgroundFill != null && Color.TryParseHex(backgroundFill, out var color))
            settings.BackgroundFill = color.ToHex();

        await Set(settings);

        var text = $"Server settings for *rank*:\n\n> **Background Fill**: #{settings.BackgroundFill}\n> **Background Image**: ";

        if (settings.BackgroundData != null)
            await Context.Interaction.RespondWithFileAsync(new MemoryStream(settings.BackgroundData), settings.BackgroundName, text);
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
