using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TenberBot.Features.HighlightFeature.Data.Enums;
using TenberBot.Shared.Features.Extensions.Strings;

namespace TenberBot.Features.HighlightFeature.Data.Models;

[Table("HighlightWords")]
[Index(nameof(GuildId), nameof(UserId))]
public class HighlightWord
{
    [Key]
    public int HighlightWordId { get; set; }

    public ulong GuildId { get; set; }

    public ulong UserId { get; set; }

    public MatchLocation MatchLocation { get; set; }

    public string Word { get; set; } = "";

    public string GetText()
    {
        var word = Word.SanitizeMD();

        return MatchLocation switch
        {
            MatchLocation.Exact => word,
            MatchLocation.AtStart => @$"~~\*~~{word}",
            MatchLocation.AtEnd => @$"{word}~~\*~~",
            MatchLocation.Anywhere => $@"~~\*~~{word}~~\*~~",
            _ => throw new NotImplementedException(),
        };
    }

    public void SetMatchLocation(bool atStart, bool atEnd)
    {
        if (atStart && atEnd)
            MatchLocation = MatchLocation.Anywhere;
        else
        {
            if (atStart)
                MatchLocation = MatchLocation.AtStart;

            if (atEnd)
                MatchLocation = MatchLocation.AtEnd;
        }
    }
}
