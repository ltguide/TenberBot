﻿using Discord;
using Discord.Interactions;
using TenberBot.Features.BotStatusFeature.Data.InteractionParents;
using TenberBot.Features.BotStatusFeature.Data.Models;
using TenberBot.Features.BotStatusFeature.Data.Services;
using TenberBot.Features.BotStatusFeature.Modals.BotStatus;
using TenberBot.Shared.Features.Data.Services;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;
using TenberBot.Shared.Features.Extensions.Strings;

namespace TenberBot.Features.BotStatusFeature.Modules.Interaction;

[RequireUserPermission(GuildPermission.ManageGuild)]
[EnabledInDm(false)]
public class BotStatusInteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IBotStatusDataService botStatusDataService;
    private readonly IInteractionParentDataService interactionParentDataService;

    public BotStatusInteractionModule(
        IBotStatusDataService botStatusDataService,
        IInteractionParentDataService interactionParentDataService)
    {
        this.botStatusDataService = botStatusDataService;
        this.interactionParentDataService = interactionParentDataService;
    }

    [ComponentInteraction("bot-status:add,*")]
    public async Task BotStatusAdd(ulong messageId)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParents.Embed, messageId);
        if (parent == null)
            return;

        await Context.Interaction.RespondWithModalAsync<BotStatusAddModal>($"bot-status:add,{messageId}");
    }

    [ModalInteraction("bot-status:add,*")]
    public async Task BotStatusAddModalResponse(ulong messageId, BotStatusAddModal modal)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParents.Embed, messageId);
        if (parent == null)
            return;

        var botStatus = new BotStatus { Text = modal.Text.SanitizeMD() };

        await botStatusDataService.Add(botStatus);

        await RespondAsync($"{Context.User.Mention} added bot status #{botStatus.BotStatusId} - {botStatus.Text}", allowedMentions: AllowedMentions.None);

        await UpdateOriginalMessage(messageId);
    }

    [ComponentInteraction("bot-status:delete,*")]
    public async Task BotStatusDelete(ulong messageId)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParents.Embed, messageId);
        if (parent == null)
            return;

        await Context.Interaction.RespondWithModalAsync<BotStatusDeleteModal>($"bot-status:delete,{messageId}");
    }

    [ModalInteraction("bot-status:delete,*")]
    public async Task BotStatusDeleteModalResponse(ulong messageId, BotStatusDeleteModal modal)
    {
        var parent = await interactionParentDataService.GetByMessageId(InteractionParents.Embed, messageId);
        if (parent == null)
            return;

        var botStatus = await botStatusDataService.GetById(modal.Text);
        if (botStatus == null)
        {
            await RespondAsync($"I couldn't find bot status #{modal.Text}.", ephemeral: true);
            return;
        }

        await botStatusDataService.Delete(botStatus);

        await RespondAsync($"{Context.User.Mention} deleted bot status #{botStatus.BotStatusId} - {botStatus.Text}", allowedMentions: AllowedMentions.None);

        await UpdateOriginalMessage(messageId);
    }

    private async Task UpdateOriginalMessage(ulong messageId)
    {
        var embed = await botStatusDataService.GetAllAsEmbed();

        await Context.Channel.GetAndModify(messageId, x => x.Embed = embed);
    }
}
