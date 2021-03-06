using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using System.Text.RegularExpressions;
using TenberBot.Data.Models;
using TenberBot.Data.Services;
using TenberBot.Data.Settings.Channel;
using TenberBot.Data.Settings.Server;
using TenberBot.Services;

namespace TenberBot.Handlers;

public class GuildExperienceHandler : DiscordClientService
{
    private readonly static Regex Lines = new(@"\n", RegexOptions.Multiline | RegexOptions.Compiled);
    private readonly static Regex Words = new(@"\S+", RegexOptions.Multiline | RegexOptions.Compiled);

    private readonly IUserVoiceChannelDataService userVoiceChannelDataService;
    private readonly IUserLevelDataService userLevelDataService;
    private readonly CacheService cacheService;

    public GuildExperienceHandler(
        IUserVoiceChannelDataService userVoiceChannelDataService,
        IUserLevelDataService userLevelDataService,
        CacheService cacheService,
        DiscordSocketClient client,
        ILogger<GuildExperienceHandler> logger) : base(client, logger)
    {
        this.userVoiceChannelDataService = userVoiceChannelDataService;
        this.userLevelDataService = userLevelDataService;
        this.cacheService = cacheService;
    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Client.MessageReceived += MessageReceived;
        Client.GuildAvailable += GuildAvailable;
        Client.UserVoiceStateUpdated += UserVoiceStateUpdated;

        return Task.CompletedTask;
    }

    private async Task MessageReceived(SocketMessage incomingMessage)
    {
        if (incomingMessage is not SocketUserMessage message)
            return;

        if (message.Source != MessageSource.User)
            return;

        if (message.Channel is not SocketGuildChannel channel)
            return;

        if (cacheService.TryGetValue<BasicServerSettings>(channel.Guild, out var settings) && string.IsNullOrWhiteSpace(settings.Prefix) == false)
        {
            int argPos = 0;
            if (message.HasStringPrefix(settings.Prefix, ref argPos) || message.HasMentionPrefix(Client.CurrentUser, ref argPos))
                return;
        }

        if (channel is SocketThreadChannel thread)
            channel = thread.ParentChannel;

        await cacheService.Channel(channel);

        var userLevel = await GetUserLevel(channel.Guild.Id, message.Author);

        userLevel.AddMessage(
            cacheService.Get<ExperienceChannelSettings>(channel),
            message.Attachments.Count,
            Lines.Matches(message.Content).Count + 1,
            Words.Matches(message.Content).Count,
            message.Content.Length);

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
                    await VoiceDisconnected(userLevel, channel, userVoiceChannel);
            }

            await userVoiceChannelDataService.Delete(userVoiceChannel);
        }

        foreach (var voiceUser in voiceUsers.Where(x => userVoiceChannels.Any(y => y.UserId == x.Id && y.ChannelId == x.VoiceChannel.Id) == false))
        {
            Console.WriteLine($"user {voiceUser.Id} is connected {voiceUser.VoiceChannel.Name} ({voiceUser.VoiceChannel.Id}) IsVideoing:{voiceUser.IsVideoing} IsStreaming:{voiceUser.IsStreaming}");

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
            //Console.WriteLine($"user {user.Id} changed state {after.VoiceChannel!.Name} ({after.VoiceChannel.Id}) IsVideoing:{before.IsVideoing} IsStreaming:{before.IsStreaming} | IsVideoing:{after.IsVideoing} IsStreaming:{after.IsStreaming}");

            await VoiceStateUpdated(socketUser, before, after);

            return;
        }

        if (before.VoiceChannel != null)
        {
            //Console.WriteLine($"user {user.Id} disconnected {before.VoiceChannel.Name} ({before.VoiceChannel.Id})");

            var userVoiceChannel = await userVoiceChannelDataService.GetByIds(before.VoiceChannel.Id, socketUser.Id);
            if (userVoiceChannel != null)
            {
                var userLevel = await GetUserLevel(userVoiceChannel.GuildId, socketUser);

                await VoiceDisconnected(userLevel, before.VoiceChannel, userVoiceChannel);

                await userVoiceChannelDataService.Delete(userVoiceChannel);
            }
        }

        if (after.VoiceChannel != null)
        {
            //Console.WriteLine($"user {user.Id} connected {after.VoiceChannel.Name} ({after.VoiceChannel.Id})");

            if (socketUser is not SocketGuildUser user)
                return;

            // TODO add to ServerUser

            await userVoiceChannelDataService.Add(new UserVoiceChannel
            {
                GuildId = user.Guild.Id,
                ChannelId = after.VoiceChannel.Id,
                UserId = socketUser.Id,
                ConnectDate = DateTime.Now,
            });
        }
    }

    private async Task<UserLevel> GetUserLevel(ulong guildId, SocketUser user)
    {
        var userLevel = await userLevelDataService.GetByIds(guildId, user.Id);
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


    private async Task VoiceDisconnected(UserLevel userLevel, IChannel channel, UserVoiceChannel userVoiceChannel)
    {
        await cacheService.Channel(channel);

        userLevel.AddVoice(
            cacheService.Get<ExperienceChannelSettings>(channel),
            ToMinutes(userVoiceChannel.ConnectDate),
            ToMinutes(userVoiceChannel.VideoDate) + userVoiceChannel.VideoMinutes,
            ToMinutes(userVoiceChannel.StreamDate) + userVoiceChannel.StreamMinutes
        );

        await userLevelDataService.Update(userLevel, null!);
    }

    private static decimal ToMinutes(DateTime? dateTime)
    {
        if (dateTime == null)
            return 0;

        return Convert.ToDecimal(Math.Round(DateTime.Now.Subtract(dateTime.Value).TotalMinutes, 2));
    }
}
