using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TenberBot.Features.GreetingFeature.Data.Enums;

namespace TenberBot.Features.GreetingFeature.Data.Models;

[Table("Greetings")]
[Index(nameof(GreetingType))]
public class Greeting
{
    [Key]
    public int GreetingId { get; set; }

    public GreetingType GreetingType { get; set; }

    public string Text { get; set; } = "";
}
