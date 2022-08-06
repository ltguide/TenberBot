using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TenberBot.Shared.Features.Data.Models;

[Table("ChannelSettings")]
[Index(nameof(ChannelId))]
[Index(nameof(GuildId))]
public class ChannelSetting
{
    [Key]
    public int ChannelSettingId { get; set; }

    public ulong GuildId { get; set; }

    public ulong ChannelId { get; set; }

    public string Name { get; set; } = "";

    public string Value { get; set; } = "";

    public object GetValue(Type type)
    {
        var result = JsonSerializer.Deserialize(Value, type, SharedFeatures.JsonSerializerOptions);

        if (result == null)
            throw new InvalidCastException($"Unable to deserialize ServerSetting: {Name}");

        return result;
    }

    public ChannelSetting SetValue<T>(T value)
    {
        Value = JsonSerializer.Serialize(value, SharedFeatures.JsonSerializerOptions);

        return this;
    }
}
