using Discord.Interactions;

namespace TenberBot.Features.HighlightFeature.Modals.Word;

public class WordDeleteModal : IModal
{
    public string Title => "Delete Word";

    [InputLabel("Id")]
    [ModalTextInput("text", maxLength: 20)]
    public int Text { get; set; }
}
