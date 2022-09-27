using Discord;
using Discord.Addons.Hosting;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenberBot.Shared.Features.Extensions.DiscordWebSocket;

namespace TenberBot.Features.AuditFeature.Services
{
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
            if (before.VoiceChannel != null)
            {
                var embed = new EmbedBuilder
                {
                    Author = socketUser.GetEmbedAuthor("left voice"),
                    Color = Color.Red,
                    Description = "Hope to see you again!"
                }
                .WithCurrentTimestamp()
                .Build();

                Console.WriteLine(before.VoiceChannel.ConnectedUsers.Count);

                await SendToChannel(before.VoiceChannel, embed);
            }

            if (after.VoiceChannel != null)
            {
                var embed = new EmbedBuilder
                {
                    Author = socketUser.GetEmbedAuthor("joined voice"),
                    Color = Color.Green,
                    Description = after.VoiceChannel.ConnectedUsers.Count == 1 ? "You are the first one in here. Send out a voice ping!" : "Welcome to the party!",
                }
                .WithCurrentTimestamp()
                .Build();

                await SendToChannel(after.VoiceChannel, embed);
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
}
