using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenberBot.Features.HighlightFeature.Data.Models;

[Table("HighlightIgnoreChannels")]
[Index(nameof(GuildId), nameof(UserId))]
public class IgnoreChannel
{
    [Key]
    public int HighlightIgnoreChannelId { get; set; }

    public ulong GuildId { get; set; }

    public ulong UserId { get; set; }

    public ulong IgnoreChannelId { get; set; }
}
