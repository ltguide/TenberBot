using Discord.Interactions;

namespace TenberBot.Features.HighFiveFeature.Modals.HighFive;

public class HighFiveAddModal : IModal
{
    public string Title => "Add High Five: ";

    [InputLabel("Text")]
    [ModalTextInput("text", maxLength: 200)]
    public string Text { get; set; } = "";
}
