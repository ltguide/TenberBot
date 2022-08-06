using Discord.Interactions;

namespace TenberBot.Features.BotStatusFeature.Modals.BotStatus;

public class BotStatusAddModal : IModal
{
    public string Title => "Add Bot Status";

    [InputLabel("Text")]
    [ModalTextInput("text", maxLength: 20)]
    public string Text { get; set; } = "";
}
