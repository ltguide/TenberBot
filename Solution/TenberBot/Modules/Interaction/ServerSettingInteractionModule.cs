using Discord;
using Discord.Interactions;
using TenberBot.Data;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Data.Services;
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
        value ??= "";

        await Set(ServerSettings.Prefix, value, value);

        if (value == "")
            await RespondAsync($"Server setting updated.\n\n> **Prefix**: *none* I can no longer respond to chat messages.");
        else
            await RespondAsync($"Server setting updated.\n\n> **Prefix**: {value}");
    }

    [SlashCommand("emote", "Set the reaction emotes.")]
    public async Task SetEmote(SetEmoteChoice type, string value)
    {
        var iEmote = value.AsIEmote();
        if (iEmote == null)
        {
            await RespondAsync($"I dont recognize that as an emote or an emoji.", ephemeral: true);
            return;
        }

        var key = type switch
        {
            SetEmoteChoice.Success => ServerSettings.EmoteSuccess,
            SetEmoteChoice.Fail => ServerSettings.EmoteFail,
            SetEmoteChoice.Unknown => ServerSettings.EmoteBusy,
            _ => throw new NotImplementedException(),
        };

        await Set(key, value, iEmote);

        await RespondAsync($"Server setting for *emote* updated.\n\n> **{type}**: {iEmote}");
    }

    private async Task Set<T>(string name, string value, T cacheValue)
    {
        cacheService.Cache.Set(Context.Guild, name, cacheValue);

        var setting = await serverSettingDataService.GetByName(Context.Guild.Id, name);
        if (setting == null)
            await serverSettingDataService.Add(new ServerSetting { GuildId = Context.Guild.Id, Name = name, Value = value, });
        else
            await serverSettingDataService.Update(setting, new ServerSetting { Value = value, });
    }
}
