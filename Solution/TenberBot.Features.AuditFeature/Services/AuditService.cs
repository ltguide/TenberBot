using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;

namespace TenberBot.Features.AuditFeature.Services;

public class AuditService : DiscordClientService
{
    public AuditService(
        DiscordSocketClient client,
        ILogger<AuditService> logger) : base(client, logger)
    {
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Client.UserVoiceStateUpdated += UserVoiceStateUpdated;

        return Task.CompletedTask;
    }

    private async Task UserVoiceStateUpdated(SocketUser socketUser, SocketVoiceState before, SocketVoiceState after)
    {
        if (before.VoiceChannel?.Id == after.VoiceChannel?.Id)
            return;

        if (before.VoiceChannel != null)
        {
            var embed = new EmbedBuilder
            {
                Author = socketUser.GetEmbedAuthor("left voice"),
                Color = Color.Red,
                Footer = new EmbedFooterBuilder { Text = $"{socketUser.Username}#{socketUser.Discriminator}", },
            }
            .WithCurrentTimestamp()
            .Build();

            await SendToChannel(before.VoiceChannel, embed, $"Left Voice: {socketUser.GetDisplayNameSanitized()}");
        }

        if (after.VoiceChannel != null)
        {
            var embedBuilder = new EmbedBuilder
            {
                Author = socketUser.GetEmbedAuthor("joined voice"),
                Color = Color.Green,
                Footer = new EmbedFooterBuilder { Text = $"{socketUser.Username}#{socketUser.Discriminator}", },
            }
            .WithCurrentTimestamp();

            string preview;

            if (after.VoiceChannel.ConnectedUsers.Count == 1)
                preview = "You are the first one in here. Send out a voice ping!";
            else
                preview = $"Joined Voice: {socketUser.GetDisplayNameSanitized()}";

            await SendToChannel(after.VoiceChannel, embedBuilder.Build(), preview);
        }
    }

    private async Task SendToChannel(SocketTextChannel channel, Embed embed, string preview)
    {
        try
        {
            var message = await channel.SendMessageAsync(preview, embed: embed);

            _ = Task.Delay(TimeSpan.FromSeconds(10)).ContinueWith(_ => message.ModifyAsync(x => x.Content = ""));
        }
        catch (Exception)
        {
            Logger.LogWarning($"failed to send message to {channel.Name} ({channel.Id})");
        }
    }
}
