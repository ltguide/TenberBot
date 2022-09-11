using Discord.Interactions;

namespace TenberBot.Features.FortuneFeature.Modals.Fortune;

public class FortuneDeleteModal : IModal
{
    public string Title => "Delete Fortune";

    [InputLabel("Id")]
    [ModalTextInput("text", maxLength: 20)]
    public int Text { get; set; }
}
