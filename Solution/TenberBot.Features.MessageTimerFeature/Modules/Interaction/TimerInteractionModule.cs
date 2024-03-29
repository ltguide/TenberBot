﻿using Discord;
using Discord.Interactions;
using TenberBot.Features.MessageTimerFeature.Data.Enums;
using TenberBot.Features.MessageTimerFeature.Data.InteractionParents;
using TenberBot.Features.MessageTimerFeature.Data.Models;
using TenberBot.Features.MessageTimerFeature.Data.Services;
using TenberBot.Features.MessageTimerFeature.Services;
using TenberBot.Shared.Features;
using TenberBot.Shared.Features.Attributes.Modules;
using TenberBot.Shared.Features.Data.Models;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.DiscordRoot;
using TenberBot.Shared.Features.Extensions.Mentions;
using TenberBot.Shared.Features.Services;

namespace TenberBot.Features.MessageTimerFeature.Modules.Interaction;

[Group("message-timer", "Send message on a timed delay.")]
[DefaultMemberPermissions(GuildPermission.ManageGuild)]
[HelpCommand(group: "Server Management")]
[EnabledInDm(false)]
public class TimerInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private const int MaxDuration = 86400 * 90;

    private readonly MessageTimerService messageTimerService;
    private readonly IMessageTimerDataService messageTimerDataService;
    private readonly VisualWebService visualWebService;
    private readonly IInteractionParentDataService interactionParentDataService;

    public TimerInteractionModule(
        MessageTimerService messageTimerService,
        IMessageTimerDataService messageTimerDataService,
        VisualWebService visualWebService,
        IInteractionParentDataService interactionParentDataService)
    {
        this.messageTimerService = messageTimerService;
        this.messageTimerDataService = messageTimerDataService;
        this.visualWebService = visualWebService;
        this.interactionParentDataService = interactionParentDataService;
    }

    [SlashCommand("clone", "Send message on a timed delay.")]
    [HelpCommand("`<channel>` `<date-time>` `<message-id>` `[pin]`")]
    public async Task TimerClone(
        [ChannelTypes(ChannelType.Voice, ChannelType.Text)] IChannel channel,
        [Summary("date-time")] DateTime dateTime,
        [Summary("message-id")]
        string messageId,
        bool pin = false)
    {
        var duration = dateTime.Subtract(DateTime.Now);

        if (duration.TotalSeconds < 10 || duration.TotalSeconds > MaxDuration)
        {
            await RespondAsync("Sorry, the duration of a timer must be at least **10 seconds** and no more than **3 months**.", ephemeral: true);
            return;
        }

        if (ulong.TryParse(messageId, out var id) == false || await Context.Channel.GetMessageAsync(id) is not IMessage message)
        {
            await RespondAsync("I couldn't find the message to send. Is it in this channel?", ephemeral: true);
            return;
        }

        await ProcessTimer(channel, duration, message.Content, pin, message.Attachments.FirstOrDefault()?.Url);
    }

    [SlashCommand("new", "Send message on a timed delay.")]
    [HelpCommand("`<channel>` `<date-time>` `<message>` `[pin]` `[url|image]`")]
    public async Task TimerNew(
        [ChannelTypes(ChannelType.Voice, ChannelType.Text)] IChannel channel,
        [Summary("date-time")] DateTime dateTime,
        string message,
        bool pin = false,
        string? url = null,
        IAttachment? image = null)
    {
        var duration = dateTime.Subtract(DateTime.Now);

        if (duration.TotalSeconds < 10 || duration.TotalSeconds > MaxDuration)
        {
            await RespondAsync("Sorry, the duration of a timer must be at least **10 seconds** and no more than **3 months**.", ephemeral: true);
            return;
        }

        await ProcessTimer(channel, duration, message.Replace(@"\n", "\n"), pin, image?.Url ?? url);
    }

    private async Task ProcessTimer(IChannel channel, TimeSpan duration, string detail, bool pin, string? url)
    {
        var messageTimer = new MessageTimer
        {
            ChannelId = Context.Channel.Id,
            UserId = Context.User.Id,
            TargetChannelId = channel.Id,
            Detail = detail,
            Pin = pin,
            Duration = SharedFeatures.BaseDuration.AddSeconds(Math.Min(MaxDuration - 1, duration.TotalSeconds)),
            StartDate = DateTime.Now,
            FinishDate = DateTime.Now.AddSeconds(duration.TotalSeconds + 1),
        };

        if (url != null)
        {
            var file = await visualWebService.GetFileAttachment(url);
            if (file != null)
            {
                messageTimer.Filename = file.Value.FileName;
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
        var reply = await FollowupAsync($"I've set a timer to send your message to {messageTimer.TargetChannelId.GetChannelMention()}{(messageTimer.Pin ? " and pin it" : "")}. It'll go off {TimestampTag.FromDateTime(messageTimer.FinishDate.ToUniversalTime(), TimestampTagStyles.LongDateTime)}.");

        await interactionParentDataService.Add(new InteractionParent
        {
            GuildId = Context.Guild.Id,
            ChannelId = messageTimer.ChannelId,
            UserId = messageTimer.UserId,
            InteractionParentType = InteractionParents.Timer,
            MessageId = reply.Id,
        }
        .SetReference(messageTimer.MessageTimerId));

        var components = new ComponentBuilder()
            .WithButton("Cancel", $"message-timer message-timer:stop,{reply.Id}", ButtonStyle.Secondary, new Emoji("❌"));

        await reply.ModifyAsync(x => x.Components = components.Build());
    }

    [ComponentInteraction("message-timer:stop,*")]
    public async Task TimerStop(ulong messageId)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParents.Timer, messageId);
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
