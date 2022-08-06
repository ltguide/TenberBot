using Discord.Interactions;

namespace TenberBot.Features.HighFiveFeature.Modals.HighFive;

public class HighFiveDeleteModal : IModal
{
    public string Title => "Delete High Five: ";

    [InputLabel("Id")]
    [ModalTextInput("text", maxLength: 20)]
    public int Text { get; set; }
}
