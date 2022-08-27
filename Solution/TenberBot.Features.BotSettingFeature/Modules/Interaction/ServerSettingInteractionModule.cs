using Discord;
using Discord.Interactions;
using TenberBot.Features.BotEmoteFeature.Data.Enums;
using TenberBot.Shared.Features.Attributes.Modules;
using TenberBot.Shared.Features.Data.Models;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.Caches;
using TenberBot.Shared.Features.Extensions.Mentions;
using TenberBot.Shared.Features.Extensions.Strings;
using TenberBot.Shared.Features.Services;
using TenberBot.Shared.Features.Settings.Server;

namespace TenberBot.Features.CommandPrefixFeature.Modules.Interaction;

[Group("bot-setting", "Manage server settings for the bot.")]
[HelpCommand(group: "Server Management")]
[DefaultMemberPermissions(GuildPermission.ManageGuild)]
[EnabledInDm(false)]
public class BotSettingInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IServerSettingDataService serverSettingDataService;
    private readonly CacheService cacheService;

    public BotSettingInteractionModule(
        IServerSettingDataService serverSettingDataService,
        CacheService cacheService)
    {
        this.serverSettingDataService = serverSettingDataService;
        this.cacheService = cacheService;
    }

    [SlashCommand("prefix", "Configure the prefix for message commands.")]
    [HelpCommand("`[value]`")]
    public async Task Prefix(string? value = null)
    {
        var settings = cacheService.Get<BasicServerSettings>(Context.Guild);

        if (value != null)
            settings.Prefix = value == "none" ? "" : value;

        await Set(settings);

        if (settings.Prefix == "")
            await RespondAsync($"Server setting:\n\n> **Prefix**: *none*\n\nI am not able to respond to chat messages except via {Context.Client.CurrentUser.Id.GetUserMention()}.");
        else
            await RespondAsync($"Server setting:\n\n> **Prefix**: {settings.Prefix}");
    }

    [SlashCommand("emote", "Configure the reaction emotes.")]
    [HelpCommand("`[success]` `[fail]` `[busy]`")]
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
