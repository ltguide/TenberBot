using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TenberBot.Data.Enums;

namespace TenberBot.Data.Models;

[Table("Greetings")]
[Index(nameof(GreetingType))]
public class Greeting
{
    [Key]
    public int GreetingId { get; set; }

    public GreetingType GreetingType { get; set; }

    public string Text { get; set; } = "";
}
