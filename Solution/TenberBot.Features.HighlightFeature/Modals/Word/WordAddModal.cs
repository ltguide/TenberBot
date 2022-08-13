using Discord.Interactions;

namespace TenberBot.Features.HighlightFeature.Modals.Word;

public class WordAddModal : IModal
{
    public string Title => "Add Word";

    [InputLabel("Word")]
    [ModalTextInput("text", maxLength: 50)]
    public string Text { get; set; } = "";
}
