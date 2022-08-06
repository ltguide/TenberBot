using Discord;
using TenberBot.Shared.Features.Attributes.Settings;

namespace TenberBot.Shared.Features.Settings.Server;

[ServerSettings("emote")]
public class EmoteServerSettings
{
    public IEmote Success { get; set; } = new Emoji("👍");

    public IEmote Fail { get; set; } = new Emoji("👎");

    public IEmote Busy { get; set; } = new Emoji("⌛");
}
