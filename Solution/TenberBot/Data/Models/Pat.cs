using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TenberBot.Data.Enums;

namespace TenberBot.Data.Models;

[Table("Pats")]
[Index(nameof(PatType))]
public class Pat
{
    [Key]
    public int PatId { get; set; }

    public PatType PatType { get; set; }

    public string Text { get; set; } = "";
}
