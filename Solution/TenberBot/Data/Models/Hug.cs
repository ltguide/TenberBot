using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TenberBot.Data.Enums;

namespace TenberBot.Data.Models;

[Table("Hugs")]
[Index(nameof(HugType))]
public class Hug
{
    [Key]
    public int HugId { get; set; }

    public HugType HugType { get; set; }

    public string Text { get; set; } = "";
}
