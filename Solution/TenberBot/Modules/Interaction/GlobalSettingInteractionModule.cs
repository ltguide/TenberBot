using Discord;
using Discord.Interactions;
using TenberBot.Data;
using TenberBot.Data.Enums;
using TenberBot.Data.Services;

namespace TenberBot.Modules.Interaction;

[Group("global-setting", "Manage global settings for the bot.")]
[RequireUserPermission(GuildPermission.ManageGuild)]
public class GlobalSettingInteractionModule : InteractionModuleBase<SocketInteractionContext>
{

    private readonly IGlobalSettingDataService globalSettingDataService;

    public GlobalSettingInteractionModule(
        IGlobalSettingDataService globalSettingDataService)
    {
        this.globalSettingDataService = globalSettingDataService;
    }

    [SlashCommand("prefix", "Set the prefix for message commands.")]
    public async Task SetPrefix(string? value = null)
    {
        value ??= "";

        GlobalSettings.Prefix = value;
        await globalSettingDataService.Set("prefix", value);


        if (value == "")
            await RespondAsync($"Global setting updated.\n\n> **Prefix**: *none* I can no longer respond to chat messages.");
        else
            await RespondAsync($"Global setting updated.\n\n> **Prefix**: {value}");
    }

    [SlashCommand("emote", "Set the reaction emotes.")]
    public async Task SetEmote(SetEmoteChoice type, string value)
    {
        IEmote? newEmote;

        if (Emote.TryParse(value, out var emote))
            newEmote = emote;
        else if (Emoji.TryParse(value, out var emoji))
            newEmote = emoji;
        else
        {
            await RespondAsync($"I dont recognize that as an emote or an emoji.", ephemeral: true);
            return;
        }

        switch (type)
        {
            case SetEmoteChoice.Success:
                GlobalSettings.EmoteSuccess = newEmote;
                await globalSettingDataService.Set("emote-success", value);
                break;

            case SetEmoteChoice.Fail:
                GlobalSettings.EmoteFail = newEmote;
                await globalSettingDataService.Set("emote-fail", value);
                break;

            case SetEmoteChoice.Unknown:
                GlobalSettings.EmoteUnknown = newEmote;
                await globalSettingDataService.Set("emote-unknown", value);
                break;
        }

        await RespondAsync($"Global setting for *emote* updated.\n\n> **{type}**: {newEmote}");
    }
}
