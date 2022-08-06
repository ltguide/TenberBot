using Discord;
using TenberBot.Shared.Features.Extensions.Strings;

namespace TenberBot.Shared.Features.Extensions.DiscordEmbed;

public static class EmbedBuilderExtensions
{
    public static EmbedBuilder AddFieldChunk(this EmbedBuilder embedBuilder, string name, string? value, bool inline = false, string delimiter = "\n")
    {
        if (value != null)
            embedBuilder.WithFields(value.ChunkBy(1024, delimiter).Select(x => new EmbedFieldBuilder { Name = name, Value = x, IsInline = inline, }));

        return embedBuilder;
    }

    public static EmbedBuilder AddFieldChunkByComma(this EmbedBuilder embedBuilder, string name, string? value, bool inline = false)
    {
        return embedBuilder.AddFieldChunk(name, value, inline, ", ");
    }
}
