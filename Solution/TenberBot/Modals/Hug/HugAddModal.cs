using Discord.Interactions;

namespace TenberBot.Modals.Hug;

public class HugAddModal : IModal
{
    public string Title => "Add Hug: ";

    [InputLabel("Text")]
    [ModalTextInput("text", maxLength: 200)]
    public string Text { get; set; } = "";
}
