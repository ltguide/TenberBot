using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenberBot.Data.Models;

[Table("ChannelSettings")]
public class ChannelSetting
{
    [Key]
    public int ChannelSettingId { get; set; }

    public ulong GuildId { get; set; }

    public ulong ChannelId { get; set; }

    public string Name { get; set; } = "";

    public string Value { get; set; } = "";
}
