using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenberBot.Data.Models;

[Table("BotStatuses")]
public class BotStatus
{
    [Key]
    public int BotStatusId { get; set; }

    public string Text { get; set; } = "";
}
