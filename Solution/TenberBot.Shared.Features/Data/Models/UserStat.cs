using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenberBot.Shared.Features.Data.Models;

[Table("UserStats")]
[Index(nameof(GuildId), nameof(UserId))]
public class UserStat
{
    [Key]
    public int UserStatId { get; set; }

    public ulong GuildId { get; set; }

    public ulong UserId { get; set; }

    public string UserStatType { get; set; } = "";

    public int Value { get; set; }
}
