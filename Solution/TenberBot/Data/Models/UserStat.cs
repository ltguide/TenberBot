using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TenberBot.Data.Enums;

namespace TenberBot.Data.Models;

[Table("UserStats")]
[Index(nameof(GuildId), nameof(UserId), IsUnique = true)]
public class UserStat
{
    [Key]
    public int UserStatId { get; set; }

    public ulong GuildId { get; set; }

    public ulong UserId { get; set; }

    public int Greetings { get; set; }

    public int HugsGiven { get; set; }

    public int HugsReceived { get; set; }

    public int SprintsCreated { get; set; }

    public int SprintsJoined { get; set; }

    public int CoinFlips { get; set; }

    public int CoinFlipStreak { get; set; }

    public VisualType? CoinFlipPrevious { get; set; }

    public int CoinFlipRecord { get; set; }

    public string CoinFlipStreakText => $"{CoinFlipStreak} flip{(CoinFlipStreak != 1 ? "s" : "")}";

    public int TimersCreated { get; set; }
}
