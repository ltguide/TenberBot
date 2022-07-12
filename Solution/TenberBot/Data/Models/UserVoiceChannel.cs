using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenberBot.Data.Models;

[Table("UserVoiceChannels")]
[Index(nameof(ChannelId), nameof(UserId), IsUnique = true)]
[Index(nameof(GuildId))]
public class UserVoiceChannel
{
    [Key]
    public int UserVoiceChannelId { get; set; }

    public ulong GuildId { get; set; }

    public ulong ChannelId { get; set; }

    public ulong UserId { get; set; }

    public DateTime ConnectDate { get; set; }

    public DateTime? VideoDate { get; set; }

    [Precision(7, 2)]
    public decimal VideoMinutes { get; set; }

    public DateTime? StreamDate { get; set; }

    [Precision(7, 2)]
    public decimal StreamMinutes { get; set; }
}
