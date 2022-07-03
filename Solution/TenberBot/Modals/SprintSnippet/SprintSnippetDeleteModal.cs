using Discord.Interactions;

namespace TenberBot.Modals.SprintSnippet;

public class SprintSnippetDeleteModal : IModal
{
    public string Title => "Delete Sprint Snippet: ";

    [InputLabel("Id")]
    [ModalTextInput("text", maxLength: 20)]
    public int Text { get; set; }
}
