using Discord;
using Discord.Interactions;
using TenberBot.Data.Enums;
using TenberBot.Data.Models;
using TenberBot.Data.Services;

namespace TenberBot.Modules.Interaction;

public class TimerInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IUserTimerDataService userTimerDataService;
    private readonly IInteractionParentDataService interactionParentDataService;
    private readonly IUserStatDataService userStatDataService;

    public TimerInteractionModule(
        IUserTimerDataService userTimerDataService,
        IInteractionParentDataService interactionParentDataService,
        IUserStatDataService userStatDataService)
    {
        this.userTimerDataService = userTimerDataService;
        this.interactionParentDataService = interactionParentDataService;
        this.userStatDataService = userStatDataService;
    }

    [ComponentInteraction("user-timer:stop,*")]
    public async Task TimerStop(ulong messageId)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.UserTimer, messageId);
        if (parent == null)
            return;

        var userTimer = await userTimerDataService.GetById(parent.GetReference<int>());
        if (userTimer == null)
            return;

        if (userTimer.UserId == Context.User.Id)
        {
            await userTimerDataService.Update(userTimer, new UserTimer { UserTimerStatus = UserTimerStatus.Stopped, });

            await interactionParentDataService.Delete(parent);

            await DeferAsync();

            await ModifyOriginalResponseAsync(x =>
            {
                x.Content += "Your timer has been stopped as requested.";
                x.Components = new ComponentBuilder().Build();
            });
        }
        else
            await RespondAsync("Sorry, you can't interact with this message.", ephemeral: true);
    }
}
