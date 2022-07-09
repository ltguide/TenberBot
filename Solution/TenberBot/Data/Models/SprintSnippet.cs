using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TenberBot.Data.Enums;

namespace TenberBot.Data.Models;

[Table("SprintsSnippets")]
[Index(nameof(SprintSnippetType))]
public class SprintSnippet
{
    [Key]
    public int SprintSnippetId { get; set; }

    public SprintSnippetType SprintSnippetType { get; set; }

    public string Text { get; set; } = "";
}
