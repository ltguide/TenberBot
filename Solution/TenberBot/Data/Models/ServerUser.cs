using Discord.WebSocket;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TenberBot.Extensions;

namespace TenberBot.Data.Models;

[Table("ServerUsers")]
public class ServerUser
{
    [Key]
    public ulong GuildId { get; set; }

    [Key]
    public ulong UserId { get; set; }

    public DateTime LastSeen { get; set; }

    public string Username { get; set; } = "";

    public string Discriminator { get; set; } = "";

    public string DisplayName { get; set; } = "";

    public string AvatarUrl { get; set; } = "";

    public ServerUser()
    {
    }

    public ServerUser(SocketUser user)
    {
        UserId = user.Id;

        Clone(user);
    }

    public void Clone(SocketUser user)
    {
        if (UserId != user.Id)
            throw new ArgumentOutOfRangeException(nameof(user));

        LastSeen = DateTime.Now;
        Username = user.Username;
        Discriminator = user.Discriminator;
        DisplayName = user.GetDisplayName();
        AvatarUrl = user.GetCurrentAvatarUrl();
    }
}
