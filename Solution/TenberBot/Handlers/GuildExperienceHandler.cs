using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;
using TenberBot.Data;
using TenberBot.Data.Models;
using TenberBot.Data.POCO;
using TenberBot.Data.Services;
using TenberBot.Extensions;
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

    private async Task GuildAvailable(SocketGuild guild)
    {
        var voiceUsers = guild.VoiceChannels.SelectMany(x => x.ConnectedUsers).ToList();

        var userVoiceChannels = await userVoiceChannelDataService.GetAllByGuildId(guild.Id);

        foreach (var userVoiceChannel in userVoiceChannels.Where(x => voiceUsers.Any(y => y.Id == x.UserId && y.VoiceChannel.Id == x.ChannelId) == false))
        {
            //Console.WriteLine($"user {userVoiceChannel.UserId} is disconnected {userVoiceChannel.ChannelId}");

            var channel = guild.VoiceChannels.FirstOrDefault(x => x.Id == userVoiceChannel.ChannelId);
            if (channel != null)
                await VoiceDisconnected(channel, userVoiceChannel);

            await userVoiceChannelDataService.Delete(userVoiceChannel);
        }

        foreach (var voiceUser in voiceUsers.Where(x => userVoiceChannels.Any(y => y.UserId == x.Id && y.ChannelId == x.VoiceChannel.Id) == false))
        {
            //Console.WriteLine($"user {voiceUser.Id} is connected {voiceUser.VoiceChannel.Name} ({voiceUser.VoiceChannel.Id})");

            await userVoiceChannelDataService.Add(new UserVoiceChannel { GuildId = guild.Id, ChannelId = voiceUser.VoiceChannel.Id, UserId = voiceUser.Id, ConnectDate = DateTime.Now });
        }
    }

    private async Task UserVoiceStateUpdated(SocketUser socketUser, SocketVoiceState before, SocketVoiceState after)
    {
        if (socketUser is not SocketGuildUser user)
            return;

        if (before.VoiceChannel != null)
        {
            //Console.WriteLine($"user {user.Id} disconnected {before.VoiceChannel.Name} ({before.VoiceChannel.Id})");

            var userVoiceChannel = await userVoiceChannelDataService.GetByIds(before.VoiceChannel.Id, user.Id);
            if (userVoiceChannel != null)
            {
                await VoiceDisconnected(before.VoiceChannel, userVoiceChannel);

                await userVoiceChannelDataService.Delete(userVoiceChannel);
            }
        }

        if (after.VoiceChannel != null)
        {
            //Console.WriteLine($"user {user.Id} connected {after.VoiceChannel.Name} ({after.VoiceChannel.Id})");

            await userVoiceChannelDataService.Add(new UserVoiceChannel { GuildId = user.Guild.Id, ChannelId = after.VoiceChannel.Id, UserId = user.Id, ConnectDate = DateTime.Now });
        }
    }

    private async Task VoiceDisconnected(IChannel channel, UserVoiceChannel userVoiceChannel)
    {
        await cacheService.Channel(channel);

        var userLevel = await userLevelDataService.GetByIds(userVoiceChannel.GuildId, userVoiceChannel.UserId);
        if (userLevel == null)
        {
            userLevel = new UserLevel { GuildId = userVoiceChannel.GuildId, UserId = userVoiceChannel.UserId };
            await userLevelDataService.Add(userLevel);
        }

        userLevel.AddVoice(
            new ChannelExperience(channel, cacheService.Cache),
            (int)Math.Ceiling(DateTime.Now.Subtract(userVoiceChannel.ConnectDate).TotalMinutes));

        await userLevelDataService.Update(userLevel, null!);
    }

    private async Task MessageReceived(SocketMessage incomingMessage)
    {
        if (incomingMessage is not SocketUserMessage message)
            return;

        if (message.Source != MessageSource.User)
            return;

        if (message.Channel is not SocketGuildChannel)
            return;

        var context = new SocketCommandContext(Client, message);

        if (cacheService.Cache.TryGetValue(context.Guild, ServerSettings.Prefix, out string prefix) && string.IsNullOrWhiteSpace(prefix) == false)
        {
            int argPos = 0;
            if (message.HasStringPrefix(prefix, ref argPos) || message.HasMentionPrefix(Client.CurrentUser, ref argPos))
                return;
        }

        await cacheService.Channel(context.Channel);

        var userLevel = await userLevelDataService.GetByContext(context);
        if (userLevel == null)
        {
            userLevel = new UserLevel { GuildId = context.Guild.Id, UserId = context.User.Id };
            await userLevelDataService.Add(userLevel);
        }

        userLevel.AddMessage(
            new ChannelExperience(context.Channel, cacheService.Cache),
            message.Attachments.Count,
            Lines.Matches(message.Content).Count + 1,
            Words.Matches(message.Content).Count,
            message.Content.Length);

        await userLevelDataService.Update(userLevel, null!);
    }
}
