﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TenberBot.Data.Enums;

namespace TenberBot.Data.Models;

[Table("HighFives")]
[Index(nameof(HighFiveType))]
public class HighFive
{
    [Key]
    public int HighFiveId { get; set; }

    public HighFiveType HighFiveType { get; set; }

    public string Text { get; set; } = "";
}
