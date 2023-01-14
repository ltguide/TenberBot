using Discord;
using Discord.Interactions;
using TenberBot.Features.UserTimerFeature.Data.Enums;
using TenberBot.Features.UserTimerFeature.Data.InteractionParents;
using TenberBot.Features.UserTimerFeature.Data.Models;
using TenberBot.Features.UserTimerFeature.Data.Services;
using TenberBot.Shared.Features.Data.Services;

namespace TenberBot.Features.UserTimerFeature.Modules.Interaction;

[EnabledInDm(false)]
public class TimerInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IUserTimerDataService userTimerDataService;
    private readonly IInteractionParentDataService interactionParentDataService;

    public TimerInteractionModule(
        IUserTimerDataService userTimerDataService,
        IInteractionParentDataService interactionParentDataService)
    {
        this.userTimerDataService = userTimerDataService;
        this.interactionParentDataService = interactionParentDataService;
    }

    [ComponentInteraction("user-timer:stop,*")]
    public async Task TimerStop(ulong messageId)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParents.Timer, messageId);
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
