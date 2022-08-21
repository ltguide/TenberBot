namespace TenberBot.Shared.Features.Data.POCO;

public class PageView
{
    public int PerPage { get; set; } = 20;

    public int CurrentPage { get; set; }

    public int PageCount { get; set; }
    public int BaseIndex => (PerPage * CurrentPage) + 1;

    public int CalcPages(decimal itemCount) => (int)Math.Ceiling(itemCount / PerPage) - 1;
}
