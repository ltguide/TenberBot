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

            await SendToChannel(before.VoiceChannel, embed);
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

            if (after.VoiceChannel.ConnectedUsers.Count == 1)
                embedBuilder.WithDescription("You are the first one in here. Send out a voice ping!");

            await SendToChannel(after.VoiceChannel, embedBuilder.Build());
        }
    }

    private async Task SendToChannel(SocketTextChannel channel, Embed embed)
    {
        try
        {
            await channel.SendMessageAsync(embed: embed);
        }
        catch (Exception)
        {
            Logger.LogWarning($"failed to send message to {channel.Name} ({channel.Id})");
        }
    }
}
