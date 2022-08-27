using Discord;
using Discord.Interactions;
using TenberBot.Features.MessageTimerFeature.Data.Enums;
using TenberBot.Features.MessageTimerFeature.Data.Models;
using TenberBot.Features.MessageTimerFeature.Data.Services;
using TenberBot.Features.MessageTimerFeature.Services;
using TenberBot.Shared.Features;
using TenberBot.Shared.Features.Attributes.Modules;
using TenberBot.Shared.Features.Data.Enums;
using TenberBot.Shared.Features.Data.Models;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.DiscordRoot;
using TenberBot.Shared.Features.Extensions.Mentions;
using TenberBot.Shared.Features.Services;

namespace TenberBot.Features.MessageTimerFeature.Modules.Interaction;

[DefaultMemberPermissions(GuildPermission.ManageGuild)]
[HelpCommand(group: "Server Management")]
[EnabledInDm(false)]
public class TimerInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private const int MaxDuration = 86400 * 90;

    private readonly MessageTimerService messageTimerService;
    private readonly IMessageTimerDataService messageTimerDataService;
    private readonly WebService webService;
    private readonly IInteractionParentDataService interactionParentDataService;

    public TimerInteractionModule(
        MessageTimerService messageTimerService,
        IMessageTimerDataService messageTimerDataService,
        WebService webService,
        IInteractionParentDataService interactionParentDataService)
    {
        this.messageTimerService = messageTimerService;
        this.messageTimerDataService = messageTimerDataService;
        this.webService = webService;
        this.interactionParentDataService = interactionParentDataService;
    }

    [SlashCommand("message-timer", "Send message on a timed delay.")]
    [HelpCommand("`<channel>` `<message>` `<date-time>` `[image]`")]
    public async Task Timer(
        [ChannelTypes(ChannelType.Voice, ChannelType.Text)] IChannel channel,
        string message,
        [Summary("date-time")] DateTime dateTime,
        IAttachment? image = null)
    {
        var duration = dateTime.Subtract(DateTime.Now);

        if (duration.TotalSeconds < 10 || duration.TotalSeconds > MaxDuration)
        {
            await RespondAsync("Sorry, the duration of a timer must be at least **10 seconds** and no more than **3 months**.", ephemeral: true);
            return;
        }

        var messageTimer = new MessageTimer
        {
            ChannelId = Context.Channel.Id,
            UserId = Context.User.Id,
            TargetChannelId = channel.Id,
            StartDate = DateTime.Now,
            FinishDate = DateTime.Now.AddSeconds(duration.TotalSeconds + 1),
            Duration = SharedFeatures.BaseDuration.AddSeconds(Math.Min(MaxDuration - 1, duration.TotalSeconds)),
            Detail = message.Replace(@"\n", "\n"),
        };

        if (image != null)
        {
            var file = await webService.GetFileAttachment(image.Url);
            if (file != null)
            {
                messageTimer.Filename = image.Filename;
                messageTimer.Data = file.Value.GetBytes();
            }
        }

        await messageTimerDataService.Add(messageTimer);

        if (messageTimer.Data == null)
            await RespondAsync(messageTimer.Detail, allowedMentions: AllowedMentions.None);
        else
            await RespondWithFileAsync(messageTimer.AsAttachment(), messageTimer.Detail, allowedMentions: AllowedMentions.None);

        await SendEmbed(messageTimer);

        messageTimerService.Cycle();
    }

    private async Task SendEmbed(MessageTimer messageTimer)
    {
        var reply = await FollowupAsync($"I've set a timer to send your message to {messageTimer.TargetChannelId.GetChannelMention()}. It'll go off {TimestampTag.FromDateTime(messageTimer.FinishDate.ToUniversalTime(), TimestampTagStyles.LongDateTime)}.");

        // TODO revisit during InteractionParent revamp because we cant clear the first timer if a second is added (Set overwrites)

        var parent = await interactionParentDataService.Set(new InteractionParent
        {
            GuildId = Context.Guild.Id,
            ChannelId = messageTimer.ChannelId,
            UserId = messageTimer.UserId,
            InteractionParentType = InteractionParentType.MessageTimer,
            MessageId = reply.Id,
        }
        .SetReference(messageTimer.MessageTimerId));

        var components = new ComponentBuilder()
            .WithButton("Cancel", $"message-timer:stop,{reply.Id}", ButtonStyle.Secondary, new Emoji("❌"));

        await reply.ModifyAsync(x => x.Components = components.Build());
    }

    [ComponentInteraction("message-timer:stop,*")]
    public async Task TimerStop(ulong messageId)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParentType.MessageTimer, messageId);
        if (parent == null)
            return;

        var messageTimer = await messageTimerDataService.GetById(parent.GetReference<int>());
        if (messageTimer == null)
            return;

        await messageTimerDataService.Update(messageTimer, new MessageTimer { MessageTimerStatus = MessageTimerStatus.Stopped, });

        await interactionParentDataService.Delete(parent);

        await DeferAsync();

        await ModifyOriginalResponseAsync(x =>
        {
            x.Content += "Your timer has been stopped as requested.";
            x.Components = new ComponentBuilder().Build();
        });
    }
}
