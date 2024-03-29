﻿using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using TenberBot.Features.ExperienceFeature.Data.Enums;
using TenberBot.Features.ExperienceFeature.Data.Models;
using TenberBot.Features.ExperienceFeature.Data.POCO;
using TenberBot.Features.ExperienceFeature.Data.Services;
using TenberBot.Features.ExperienceFeature.Settings.Channel;
using TenberBot.Shared.Features.Services;

namespace TenberBot.Features.ExperienceFeature.Services;

public class GuildExperienceService : DiscordClientService, IGuildMessageService
{
    private readonly static Regex Lines = new(@"\n", RegexOptions.Multiline | RegexOptions.Compiled);
    private readonly static Regex Words = new(@"\S+", RegexOptions.Multiline | RegexOptions.Compiled);

    private readonly IUserVoiceChannelDataService userVoiceChannelDataService;
    private readonly IUserLevelDataService userLevelDataService;
    private readonly CacheService cacheService;

    public GuildExperienceService(
        IUserVoiceChannelDataService userVoiceChannelDataService,
        IUserLevelDataService userLevelDataService,
        CacheService cacheService,
        DiscordSocketClient client,
        ILogger<GuildExperienceService> logger) : base(client, logger)
    {
        this.userVoiceChannelDataService = userVoiceChannelDataService;
        this.userLevelDataService = userLevelDataService;
        this.cacheService = cacheService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Client.GuildAvailable += GuildAvailable;
        Client.UserVoiceStateUpdated += UserVoiceStateUpdated;

        return Task.CompletedTask;
    }

    public async Task Handle(SocketGuildChannel channel, SocketUserMessage message)
    {
        try
        {
            if (channel is SocketThreadChannel thread)
                channel = thread.ParentChannel;

            var userLevel = await GetUserLevel(channel.Guild.Id, message.Author);

            var mode = cacheService.Get<ExperienceChannelSettings>(channel).Mode;

            var stats = new MessageStats(
                message.Attachments.Count,
                Lines.Matches(message.Content).Count + 1,
                Words.Matches(message.Content).Count,
                message.Content.Length
            );

            userLevel.AddStats(mode.HasFlag(ExperienceModes.Normal), cacheService.Get<NormalExperienceModeChannelSettings>(channel), stats);

            if (mode.HasFlag(ExperienceModes.EventA))
                userLevel.AddStats("EventA", cacheService.Get<EventExperienceModeChannelSettings>(channel), stats);

            else if (mode.HasFlag(ExperienceModes.EventB))
                userLevel.AddStats("EventB", cacheService.Get<EventExperienceModeChannelSettings>(channel), stats);

            await userLevelDataService.Update(userLevel, null!);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "");
        }
    }

    public Task<UserLevel?> GetUserLevel(ulong guildId, ulong userId)
    {
        return userLevelDataService.GetByIds(guildId, userId);
    }

    private async Task<UserLevel> GetUserLevel(ulong guildId, SocketUser user)
    {
        var userLevel = await GetUserLevel(guildId, user.Id);
        if (userLevel == null)
        {
            userLevel = new UserLevel
            {
                GuildId = guildId,
                UserId = user.Id,
                ServerUser = new ServerUser(user) { GuildId = guildId, },
            };

            await userLevelDataService.Add(userLevel);
        }
        else
            userLevel.ServerUser.Clone(user);

        return userLevel;
    }

    public Task SetEventExperience(string eventName, UserLevel dbUserLevel, decimal experience)
    {
        if (eventName == "EventA")
            dbUserLevel.EventAExperience = experience;
        else
            dbUserLevel.EventBExperience = experience;

        return userLevelDataService.Update(dbUserLevel, null!);
    }

    public Task ResetEventExperience(string eventName, ulong guildId)
    {
        if (eventName == "EventA")
            return userLevelDataService.ResetEventAExperience(guildId);

        return userLevelDataService.ResetEventBExperience(guildId);
    }

    private async Task AddVoiceExperience(UserLevel userLevel, IChannel channel, UserVoiceChannel userVoiceChannel)
    {
        await cacheService.Channel(channel);

        var mode = cacheService.Get<ExperienceChannelSettings>(channel).Mode;

        var stats = new VoiceStats(
            ToMinutes(userVoiceChannel.ConnectDate),
            ToMinutes(userVoiceChannel.VideoDate) + userVoiceChannel.VideoMinutes,
            ToMinutes(userVoiceChannel.StreamDate) + userVoiceChannel.StreamMinutes
        );

        userLevel.AddStats(mode.HasFlag(ExperienceModes.Normal), cacheService.Get<NormalExperienceModeChannelSettings>(channel), stats);

        if (mode.HasFlag(ExperienceModes.EventA))
            userLevel.AddStats("EventA", cacheService.Get<EventExperienceModeChannelSettings>(channel), stats);

        else if (mode.HasFlag(ExperienceModes.EventB))
            userLevel.AddStats("EventB", cacheService.Get<EventExperienceModeChannelSettings>(channel), stats);

        await userLevelDataService.Update(userLevel, null!);
    }

    private async Task GuildAvailable(SocketGuild guild)
    {
        var voiceUsers = guild.VoiceChannels.SelectMany(x => x.ConnectedUsers).ToList();

        var userVoiceChannels = await userVoiceChannelDataService.GetAllByGuildId(guild.Id);

        foreach (var userVoiceChannel in userVoiceChannels.Where(x => voiceUsers.Any(y => y.Id == x.UserId && y.VoiceChannel.Id == x.ChannelId) == false))
        {
            //Console.WriteLine($"user {userVoiceChannel.UserId} is disconnected {userVoiceChannel.ChannelId}");

            var channel = guild.VoiceChannels.FirstOrDefault(x => x.Id == userVoiceChannel.ChannelId);
            if (channel != null)
            {
                var userLevel = await userLevelDataService.GetByIds(guild.Id, userVoiceChannel.UserId);
                if (userLevel != null)
                    await AddVoiceExperience(userLevel, channel, userVoiceChannel);
            }

            await userVoiceChannelDataService.Delete(userVoiceChannel);
        }

        foreach (var voiceUser in voiceUsers.Where(x => userVoiceChannels.Any(y => y.UserId == x.Id && y.ChannelId == x.VoiceChannel.Id) == false))
        {
            //Console.WriteLine($"user {voiceUser.Id} is connected {voiceUser.VoiceChannel.Name} ({voiceUser.VoiceChannel.Id}) IsVideoing:{voiceUser.IsVideoing} IsStreaming:{voiceUser.IsStreaming}");

            await userVoiceChannelDataService.Add(new UserVoiceChannel
            {
                GuildId = guild.Id,
                ChannelId = voiceUser.VoiceChannel.Id,
                UserId = voiceUser.Id,
                ConnectDate = DateTime.Now,
                VideoDate = voiceUser.IsVideoing ? DateTime.Now : null,
                StreamDate = voiceUser.IsStreaming ? DateTime.Now : null,
            });
        }
    }

    private async Task UserVoiceStateUpdated(SocketUser socketUser, SocketVoiceState before, SocketVoiceState after)
    {
        if (before.VoiceChannel?.Id == after.VoiceChannel?.Id)
        {
            //Console.WriteLine($"user {socketUser.Id} changed state {after.VoiceChannel!.Name} ({after.VoiceChannel.Id}) IsVideoing:{before.IsVideoing} IsStreaming:{before.IsStreaming} | IsVideoing:{after.IsVideoing} IsStreaming:{after.IsStreaming}");

            await VoiceStateUpdated(socketUser, before, after);

            return;
        }

        if (before.VoiceChannel != null)
        {
            //Console.WriteLine($"user {socketUser.Id} disconnected {before.VoiceChannel.Name} ({before.VoiceChannel.Id})");

            var userVoiceChannel = await userVoiceChannelDataService.GetByIds(before.VoiceChannel.Id, socketUser.Id);
            if (userVoiceChannel != null)
            {
                var userLevel = await GetUserLevel(userVoiceChannel.GuildId, socketUser);

                await AddVoiceExperience(userLevel, before.VoiceChannel, userVoiceChannel);

                await userVoiceChannelDataService.Delete(userVoiceChannel);
            }
        }

        if (after.VoiceChannel != null)
        {
            //Console.WriteLine($"user {socketUser.Id} connected {after.VoiceChannel.Name} ({after.VoiceChannel.Id})");

            if (socketUser is not SocketGuildUser user)
                return;

            await userVoiceChannelDataService.Add(new UserVoiceChannel
            {
                GuildId = user.Guild.Id,
                ChannelId = after.VoiceChannel.Id,
                UserId = socketUser.Id,
                ConnectDate = DateTime.Now,
            });
        }
    }

    public async Task VoiceStateUpdated(SocketUser socketUser, SocketVoiceState before, SocketVoiceState after)
    {
        if (before.IsVideoing == after.IsVideoing && before.IsStreaming == after.IsStreaming)
            return;

        var userVoiceChannel = await userVoiceChannelDataService.GetByIds(after.VoiceChannel!.Id, socketUser.Id);
        if (userVoiceChannel == null)
            return;

        if (before.IsVideoing != after.IsVideoing)
        {
            userVoiceChannel.VideoMinutes += ToMinutes(userVoiceChannel.VideoDate);
            userVoiceChannel.VideoDate = after.IsVideoing ? DateTime.Now : null;
        }

        if (before.IsStreaming != after.IsStreaming)
        {
            userVoiceChannel.StreamMinutes += ToMinutes(userVoiceChannel.StreamDate);
            userVoiceChannel.StreamDate = after.IsStreaming ? DateTime.Now : null;
        }

        await userVoiceChannelDataService.Update(userVoiceChannel, null!);
    }

    private static decimal ToMinutes(DateTime? dateTime)
    {
        if (dateTime == null)
            return 0;

        return Convert.ToDecimal(Math.Round(DateTime.Now.Subtract(dateTime.Value).TotalMinutes, 2));
    }
}
