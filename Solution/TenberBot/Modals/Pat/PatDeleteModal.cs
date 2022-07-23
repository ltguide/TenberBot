using Discord.Interactions;

namespace TenberBot.Modals.Pat;

public class PatDeleteModal : IModal
{
    public string Title => "Delete Pat: ";

    [InputLabel("Id")]
    [ModalTextInput("text", maxLength: 20)]
    public int Text { get; set; }
}
