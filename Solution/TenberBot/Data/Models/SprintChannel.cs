using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TenberBot.Data.Enums;

namespace TenberBot.Data.Models;

[Table("SprintsChannels")]
public class SprintChannel
{
    [Key]
    public int SprintChannelId { get; set; }

    public ulong ChannelId { get; set; }

    public SprintMode SprintMode { get; set; }

    public string Role { get; set; } = "";
}
