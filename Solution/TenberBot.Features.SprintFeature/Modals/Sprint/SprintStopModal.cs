using Discord.Interactions;

namespace TenberBot.Features.SprintFeature.Modals.Sprint;

public class SprintStopModal : IModal
{
    public string Title => "Stop Sprint Confirmation";

    [InputLabel("Type 'stop'")]
    [ModalTextInput("text", placeholder: "stop", minLength: 4, maxLength: 4)]
    public string Text { get; set; } = "";
}
