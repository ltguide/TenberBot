using Discord;
using Discord.Interactions;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Data.Services;
using TenberBot.Data.Settings.Server;
using TenberBot.Extensions;
using TenberBot.Services;

namespace TenberBot.Modules.Interaction;

[Group("server-setting", "Manage server settings for the bot.")]
[RequireUserPermission(GuildPermission.ManageGuild)]
public class ServerSettingInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IServerSettingDataService serverSettingDataService;
    private readonly CacheService cacheService;

    public ServerSettingInteractionModule(
        IServerSettingDataService serverSettingDataService,
        CacheService cacheService)
    {
        this.serverSettingDataService = serverSettingDataService;
        this.cacheService = cacheService;
    }

    [SlashCommand("prefix", "Set the prefix for message commands.")]
    public async Task SetPrefix(string? value = null)
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

    [SlashCommand("emote", "Set the reaction emotes.")]
    public async Task SetEmote(
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
