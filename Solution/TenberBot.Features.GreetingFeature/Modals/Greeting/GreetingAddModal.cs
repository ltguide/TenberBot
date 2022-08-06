using Discord.Interactions;

namespace TenberBot.Features.GreetingFeature.Modals.Greeting;

public class GreetingAddModal : IModal
{
    public string Title => "Add Greeting: ";

    [InputLabel("Text")]
    [ModalTextInput("text", maxLength: 200)]
    public string Text { get; set; } = "";
}
