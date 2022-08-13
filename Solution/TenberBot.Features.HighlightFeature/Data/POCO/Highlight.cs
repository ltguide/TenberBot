using System.Text.RegularExpressions;
using TenberBot.Features.HighlightFeature.Data.Enums;
using TenberBot.Features.HighlightFeature.Data.Models;

namespace TenberBot.Features.HighlightFeature.Data.POCO;

public class Highlight
{
    private readonly MatchLocation matchLocation;

    public Dictionary<string, List<ulong>> Words { get; private set; }
    public Regex? Pattern { get; private set; }

    public Highlight(IGrouping<(ulong GuildId, MatchLocation MatchLocation), HighlightWord> grouping)
    {
        matchLocation = grouping.Key.MatchLocation;

        Words = grouping.GroupBy(x => x.Word.ToLower()).ToDictionary(x => x.Key, x => x.Select(x => x.UserId).ToList());

        Pattern = GetPattern();
    }

    public Highlight(HighlightWord highlightWord)
    {
        matchLocation = highlightWord.MatchLocation;

        Words = new()
        {
            { highlightWord.Word.ToLower(), new List<ulong> { highlightWord.UserId } }
        };

        Pattern = GetPattern();
    }

    public void Add(HighlightWord highlightWord)
    {
        var key = highlightWord.Word.ToLower();

        if (Words.TryGetValue(key, out var values) == false)
        {
            Words.Add(key, values = new List<ulong>());

            Pattern = GetPattern();
        }

        values.Add(highlightWord.UserId);
    }

    public void Delete(HighlightWord highlightWord)
    {
        var key = highlightWord.Word.ToLower();

        if (Words.TryGetValue(key, out var values) == false)
            return;

        values.Remove(highlightWord.UserId);

        if (values.Count == 0)
        {
            Words.Remove(key);

            Pattern = GetPattern();
        }
    }

    private Regex? GetPattern()
    {
        if (Words.Count == 0)
            return null;

        var group = string.Join('|', Words.Keys.Select(x => Regex.Escape(x)));

        var pattern = matchLocation switch
        {
            MatchLocation.Exact => @$"\b({group})\b",
            MatchLocation.AtStart => @$"({group})\b",
            MatchLocation.AtEnd => @$"\b({group})",
            MatchLocation.Anywhere => $"({group})",
            _ => throw new NotImplementedException(),
        };

        return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }
}
