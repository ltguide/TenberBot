using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenberBot.Features.ExperienceFeature.Data.Models;

[Table("RankCards")]
[Index(nameof(GuildId))]
[Index(nameof(RoleId))]
public class RankCard
{
    [Key]
    public int RankCardId { get; set; }

    public ulong GuildId { get; set; }

    public ulong RoleId { get; set; }

    public byte[] Data { get; set; } = Array.Empty<byte>();

    public string Filename { get; set; } = "";

    public string GuildColor { get; set; } = "FFFFFFFF";

    public string UserColor { get; set; } = "FFFFFFFF";

    public string RoleColor { get; set; } = "FFFFFFFF";

    public string RankColor { get; set; } = "FFFFFFFF";

    public string LevelColor { get; set; } = "FFFFFFFF";

    public string ExperienceColor { get; set; } = "FFFFFFFF";

    public string ProgressColor { get; set; } = "FFFFFFFF";

    public string ProgressFill { get; set; } = "000000FF";

    [NotMapped]
    public string Name { get; set; } = "";
}
