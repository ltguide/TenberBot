using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

    public int? Reference { get; set; }
}
