using Discord.Interactions;

namespace TenberBot.Modals.Pat;

public class PatAddModal : IModal
{
    public string Title => "Add Pat: ";

    [InputLabel("Text")]
    [ModalTextInput("text", maxLength: 200)]
    public string Text { get; set; } = "";
}
