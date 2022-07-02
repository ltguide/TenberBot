using Discord.Interactions;

namespace TenberBot.Modals.Hug;

public class HugDeleteModal : IModal
{
    public string Title => "Delete Hug: ";

    [InputLabel("Id")]
    [ModalTextInput("text", maxLength: 20)]
    public int Text { get; set; }
}
