using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenberBot.Data.Models;

[Table("UserStats")]
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
}
