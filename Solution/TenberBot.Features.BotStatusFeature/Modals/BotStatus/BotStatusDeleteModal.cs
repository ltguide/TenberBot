using Discord.Interactions;

namespace TenberBot.Features.BotStatusFeature.Modals.BotStatus;

public class BotStatusDeleteModal : IModal
{
    public string Title => "Delete Bot Status";

    [InputLabel("Id")]
    [ModalTextInput("text", maxLength: 20)]
    public int Text { get; set; }
}
