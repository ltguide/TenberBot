using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenberBot.Data.Models;

[Table("GlobalSettings")]
public class GlobalSetting
{
    [Key]
    public int GlobalSettingId { get; set; }

    public string Name { get; set; } = "";

    public string Value { get; set; } = "";
}
