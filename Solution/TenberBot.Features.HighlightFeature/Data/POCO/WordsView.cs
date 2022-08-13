namespace TenberBot.Features.HighlightFeature.Data.POCO;

public class WordsView
{
    public int PerPage { get; set; } = 20;

    public int CurrentPage { get; set; }

    public int PageCount { get; set; }

    public int BaseIndex => (PerPage * CurrentPage) + 1;
}
