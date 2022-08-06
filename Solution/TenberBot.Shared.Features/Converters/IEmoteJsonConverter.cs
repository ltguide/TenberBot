using Discord;
using System.Text.Json;
using System.Text.Json.Serialization;
using TenberBot.Shared.Features.Extensions.Strings;

namespace TenberBot.Shared.Features.Converters;

public class IEmoteJsonConverter : JsonConverter<IEmote>
{
    public override IEmote Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString()?.AsIEmote() ?? new Emoji("☹");
    }

    public override void Write(Utf8JsonWriter writer, IEmote dateTimeValue, JsonSerializerOptions options)
    {
        writer.WriteStringValue(dateTimeValue.ToString());
    }
}
