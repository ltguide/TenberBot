namespace TenberBot.Data.POCO;

public abstract class Story
{
    public string Name { get; set; } = "";
    public string Author { get; set; } = "";
    public string? Summary { get; set; }
    public string Rating { get; set; } = "";
    public string Language { get; set; } = "";
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public string? Relationships { get; set; }
    public string? Characters { get; set; }
    public string Fandom { get; set; } = "";
    public string Warning { get; set; } = "";
    public string? Collections { get; set; }
    public string? Series { get; set; }
    public string Published { get; set; } = "";
    public string? StatusName { get; set; }
    public string? Status { get; set; }
    public string Words { get; set; } = "";
    public string Chapters { get; set; } = "";
    public string? Comments { get; set; }
    public string? Kudos { get; set; }
    public string? Bookmarks { get; set; }
    public string? Hits { get; set; }
}
