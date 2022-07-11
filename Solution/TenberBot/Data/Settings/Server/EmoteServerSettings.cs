using Discord;
using TenberBot.Attributes;

namespace TenberBot.Data.Settings.Server;

[ServerSettings("emote")]
public class EmoteServerSettings
{
    public IEmote Success { get; set; } = new Emoji("👍");

    public IEmote Fail { get; set; } = new Emoji("👎");

    public IEmote Busy { get; set; } = new Emoji("⌛");
}
