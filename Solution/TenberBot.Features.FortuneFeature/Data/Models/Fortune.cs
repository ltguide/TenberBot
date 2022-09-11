using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenberBot.Features.FortuneFeature.Data.Models;

[Table("Fortunes")]
public class Fortune
{
    [Key]
    public int FortuneId { get; set; }

    public string Text { get; set; } = "";
}
