using Discord.Interactions;

namespace TenberBot.Features.HighlightFeature.Modals.Ignore;

public class IgnoreDeleteModal : IModal
{
    public string Title => "Delete ";

    [InputLabel("Id")]
    [ModalTextInput("text", maxLength: 20)]
    public int Text { get; set; }
}
