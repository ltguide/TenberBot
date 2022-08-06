using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using TenberBot.Shared.Features.Data.Enums;

namespace TenberBot.Shared.Features.Data.Models;

[Table("InteractionParents")]
[Index(nameof(InteractionParentType), nameof(ChannelId), nameof(UserId), IsUnique = true)]
[Index(nameof(InteractionParentType), nameof(MessageId), IsUnique = true)]
[Index(nameof(GuildId))]
public class InteractionParent
{
    [Key]
    public int InteractionParentId { get; set; }

    public ulong GuildId { get; set; }

    public ulong ChannelId { get; set; }

    public ulong? UserId { get; set; }

    public ulong MessageId { get; set; }

    public InteractionParentType InteractionParentType { get; set; }

    public string? Reference { get; private set; }

    public T? GetReference<T>()
    {
        if (Reference == null)
            return default;

        return JsonSerializer.Deserialize<T>(Reference, SharedFeatures.JsonSerializerOptions);
    }

    public InteractionParent SetReference<T>(T value)
    {
        Reference = value == null ? null : JsonSerializer.Serialize(value, SharedFeatures.JsonSerializerOptions);

        return this;
    }

    public void Update(InteractionParent newObject)
    {
        UserId = newObject.UserId;
        MessageId = newObject.MessageId;
        Reference = newObject.Reference;
    }
}
