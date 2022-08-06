using Discord;
using Discord.Interactions;

namespace TenberBot.Features.SprintFeature.Modals.Sprint;

public class SprintJoinModal : IModal
{
    public string Title => "Join Sprint";

    [InputLabel("Message to members")]
    [ModalTextInput("text", TextInputStyle.Paragraph, placeholder: "Let's do this!", maxLength: 200)]
    public string Message { get; set; } = "";
}
