using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenberBot.Features.SprintFeature.Data.Models;

[Table("UserSprints")]
[Index(nameof(UserId))]
public class UserSprint
{
    [Key]
    public int UserSprintId { get; set; }

    public ulong UserId { get; set; }

    public int SprintId { get; set; }
    public Sprint Sprint { get; set; } = new();

    public DateTime JoinDate { get; set; }

    public string? Message { get; set; }
}
