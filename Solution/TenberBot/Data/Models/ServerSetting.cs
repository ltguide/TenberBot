﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TenberBot.Data.Models;

[Table("ServerSettings")]
[Index(nameof(GuildId))]
public class ServerSetting
{
    [Key]
    public int ServerSettingId { get; set; }

    public ulong GuildId { get; set; }

    public string Name { get; set; } = "";

    public string Value { get; set; } = "";
}
