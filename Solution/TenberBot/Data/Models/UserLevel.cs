using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenberBot.Data.Models;

[Table("UserLevels")]
[Index(nameof(GuildId), nameof(UserId), IsUnique = true)]
public class UserLevel
{
    [Key]
    public int UserLevelId { get; set; }

    public ulong GuildId { get; set; }

    public ulong UserId { get; set; }

    public int Level { get; set; }

    public long Experience { get; set; }

    public int MessageLines { get; set; }

    public int MessageAttachments { get; set; }

    public long MessageWords { get; set; }

    public long MessageCharacters { get; set; }
}
