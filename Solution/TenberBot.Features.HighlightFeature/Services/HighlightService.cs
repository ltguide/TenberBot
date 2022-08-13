using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using TenberBot.Features.HighlightFeature.Data.Enums;
using TenberBot.Features.HighlightFeature.Data.Models;
using TenberBot.Features.HighlightFeature.Data.POCO;
using TenberBot.Features.HighlightFeature.Data.Services;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;
using TenberBot.Shared.Features.Services;

namespace TenberBot.Features.HighlightFeature.Services;

public class HighlightService : DiscordClientService, IGuildMessageService
{
    private Dictionary<(ulong GuildId, ulong UserId), List<ulong>> IgnoreChannels = new();
    private Dictionary<(ulong GuildId, ulong UserId), List<ulong>> IgnoreUsers = new();

    private Dictionary<(ulong GuildId, MatchLocation MatchLocation), Highlight> Highlights = new();

    private readonly IIgnoreUserDataService ignoreUserDataService;
    private readonly IIgnoreChannelDataService ignoreChannelDataService;
    private readonly IHighlightWordDataService highlightWordDataService;

    public HighlightService(
        IIgnoreUserDataService ignoreUserDataService,
        IIgnoreChannelDataService ignoreChannelDataService,
        IHighlightWordDataService highlightWordDataService,
        DiscordSocketClient client,
        ILogger<HighlightService> logger) : base(client, logger)
    {
        this.ignoreUserDataService = ignoreUserDataService;
        this.ignoreChannelDataService = ignoreChannelDataService;
        this.highlightWordDataService = highlightWordDataService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var ignoreChannels = await ignoreChannelDataService.GetAll();
        IgnoreChannels = ignoreChannels.GroupBy(x => (x.GuildId, x.UserId)).ToDictionary(x => x.Key, x => x.Select(x => x.IgnoreChannelId).ToList());

        var ignoreUsers = await ignoreUserDataService.GetAll();
        IgnoreUsers = ignoreUsers.GroupBy(x => (x.GuildId, x.UserId)).ToDictionary(x => x.Key, x => x.Select(x => x.IgnoreUserId).ToList());

        var highlightWords = await highlightWordDataService.GetAll();
        Highlights = highlightWords.GroupBy(i => (i.GuildId, i.MatchLocation)).ToDictionary(x => x.Key, x => new Highlight(x));
    }

    public async Task Handle(SocketGuildChannel channel, SocketUserMessage message)
    {
        try
        {
            var userIds = new HashSet<ulong>();

            foreach (var matchLocation in Enum.GetValues<MatchLocation>())
            {
                if (Highlights.TryGetValue((channel.Guild.Id, matchLocation), out var highlight) == false || highlight.Pattern == null)
                    continue;

                var words = new HashSet<string>();

                foreach (Match match in highlight.Pattern.Matches(message.Content))
                    words.Add(match.Groups[1].Value.ToLower());

                foreach (var word in words)
                    userIds.UnionWith(highlight.Words[word]);
            }

            var users = new List<SocketGuildUser>();

            foreach (var userId in userIds)
            {
                if (userId == message.Author.Id)
                    continue;

                var key = (channel.Guild.Id, userId);

                if (IgnoreChannels.TryGetValue(key, out var values) && values.Contains(channel.Id))
                    continue;

                if (IgnoreUsers.TryGetValue(key, out values) && values.Contains(message.Author.Id))
                    continue;

                var user = channel.GetUser(userId);
                if (user == null)
                    continue;

                users.Add(user);
            }

            await SendNotices(users, channel, message);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "");
        }
    }

    private async Task SendNotices(List<SocketGuildUser> users, SocketGuildChannel channel, SocketUserMessage message)
    {
        if (users.Count == 0)
            return;

        var embed = new EmbedBuilder
        {
            Color = Color.Magenta,
            Author = message.Author.GetEmbedAuthor($"said something interesting in {channel.Guild.Name}"),
            Title = $"Jump to Message in #{channel.Name}",
            Url = message.GetJumpUrl(),
            Description = message.Content,
        }.Build();

        foreach (var user in users)
        {
            Logger.LogDebug($"DM to {user.Username}#{user.Discriminator} - (Highlight) {message.GetJumpUrl()}");

            var dmChannel = await user.CreateDMChannelAsync();

            try
            {
                await dmChannel.SendMessageAsync(embed: embed);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Unable to DM: {user.Id}");
            }
        }
    }

    public void Add(IgnoreChannel ignoreChannel)
    {
        var key = (ignoreChannel.GuildId, ignoreChannel.UserId);

        if (IgnoreChannels.TryGetValue(key, out var values) == false)
            IgnoreChannels.Add(key, values = new List<ulong>());

        values.Add(ignoreChannel.IgnoreChannelId);
    }

    public void Delete(IgnoreChannel ignoreChannel)
    {
        var key = (ignoreChannel.GuildId, ignoreChannel.UserId);

        if (IgnoreChannels.TryGetValue(key, out var values) == false)
            return;

        values.Remove(ignoreChannel.IgnoreChannelId);

        if (values.Count == 0)
            IgnoreChannels.Remove(key);
    }

    public void Add(IgnoreUser ignoreUser)
    {
        var key = (ignoreUser.GuildId, ignoreUser.UserId);

        if (IgnoreUsers.TryGetValue(key, out var values) == false)
            IgnoreUsers.Add(key, values = new List<ulong>());

        values.Add(ignoreUser.IgnoreUserId);
    }

    public void Delete(IgnoreUser ignoreUser)
    {
        var key = (ignoreUser.GuildId, ignoreUser.UserId);

        if (IgnoreUsers.TryGetValue(key, out var values) == false)
            return;

        values.Remove(ignoreUser.IgnoreUserId);

        if (values.Count == 0)
            IgnoreUsers.Remove(key);
    }

    public void Add(HighlightWord highlightWord)
    {
        var key = (highlightWord.GuildId, highlightWord.MatchLocation);

        if (Highlights.TryGetValue(key, out var highlight) == false)
            Highlights.Add(key, new Highlight(highlightWord));
        else
            highlight.Add(highlightWord);
    }

    public void Delete(HighlightWord highlightWord)
    {
        var key = (highlightWord.GuildId, highlightWord.MatchLocation);

        if (Highlights.TryGetValue(key, out var highlight) == false)
            return;

        highlight.Delete(highlightWord);

        if (highlight.Words.Count == 0)
            Highlights.Remove(key);
    }
}
