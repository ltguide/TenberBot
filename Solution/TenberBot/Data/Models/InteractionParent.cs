using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using TenberBot.Data.Enums;

namespace TenberBot.Data.Models;

[Table("InteractionParents")]
public class InteractionParent
{
    [Key]
    public int InteractionParentId { get; set; }

    public ulong ChannelId { get; set; }

    public ulong? UserId { get; set; }

    public ulong MessageId { get; set; }

    public InteractionParentType InteractionParentType { get; set; }

    public string? Reference { get; private set; }

    public T? GetReference<T>()
    {
        if (Reference == null)
            return default;

        return JsonSerializer.Deserialize<T>(Reference);
    }

    public InteractionParent SetReference<T>(T value)
    {
        Reference = value == null ? null : JsonSerializer.Serialize(value);

        return this;
    }

    internal void Update(InteractionParent newObject)
    {
        UserId = newObject.UserId;
        MessageId = newObject.MessageId;
        Reference = newObject.Reference;
    }
}
