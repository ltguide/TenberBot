using Discord.Interactions;

namespace TenberBot.Features.FortuneFeature.Modals.Fortune;

public class FortuneAddModal : IModal
{
    public string Title => "Add Fortune";

    [InputLabel("Text")]
    [ModalTextInput("text", maxLength: 200)]
    public string Text { get; set; } = "";
}
