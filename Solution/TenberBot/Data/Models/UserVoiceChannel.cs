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
}
