using Discord.Interactions;

namespace TenberBot.Modals.Greeting;

public class GreetingDeleteModal : IModal
{
    public string Title => "Delete Greeting";

    [InputLabel("Id")]
    [ModalTextInput("text", maxLength: 20)]
    public int Text { get; set; }
}
