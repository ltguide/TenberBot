using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenberBot.Features.HighlightFeature.Data.Models;

[Table("HighlightIgnoreUsers")]
[Index(nameof(GuildId), nameof(UserId))]
public class IgnoreUser
{
    [Key]
    public int HighlightIgnoreUserId { get; set; }

    public ulong GuildId { get; set; }

    public ulong UserId { get; set; }

    public ulong IgnoreUserId { get; set; }
}
