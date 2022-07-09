using Discord;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TenberBot.Data.Enums;
using TenberBot.Extensions;

namespace TenberBot.Data.Models;

[Table("Sprints")]
[Index(nameof(SprintStatus))]
public class Sprint
{
    [Key]
    public int SprintId { get; set; }

    public ulong ChannelId { get; set; }

    public ulong UserId { get; set; }

    public SprintStatus SprintStatus { get; set; }

    public SprintMode SprintMode { get; set; }

    public string? Detail { get; set; }

    public DateTime Duration { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime FinishDate { get; set; }

    public IList<UserSprint> Users { get; set; } = new List<UserSprint>();

    public string UserMentions => string.Join(", ", Users.Select(x => x.UserId.GetUserMention()));

    public SprintStatus? GetNextStatus()
    {
        if (SprintStatus == SprintStatus.Stopped || SprintStatus == SprintStatus.Finished)
            return null;

        if (DateTime.Now > FinishDate)
            return SprintStatus.Finished;

        if (SprintStatus == SprintStatus.Waiting && DateTime.Now > StartDate)
            return SprintStatus.Started;

        return null;
    }

    public Embed GetAsEmbed()
    {
        var users = string.Join("\n", Users.Select(x =>
            $"{x.UserId.GetUserMention()} joined at {TimestampTag.FromDateTime(x.JoinDate.ToUniversalTime(), TimestampTagStyles.ShortTime)}\n> {x.Message ?? "\u200B"}"
        ));

        var embedBuilder = new EmbedBuilder
        {
            Description = $"{Detail}*Good luck!*\n—————————————————————\n{users}",
            Timestamp = DateTime.Now,
        }
        .WithFooter($"Duration of sprint: {Duration.TimeOfDay}");

        switch (SprintStatus)
        {
            case SprintStatus.Waiting:
                embedBuilder
                    .WithColor(Color.Blue)
                    .WithTitle($"The sprint will start {TimestampTag.FromDateTime(StartDate.ToUniversalTime(), TimestampTagStyles.Relative)}");
                break;

            case SprintStatus.Started:
                embedBuilder
                    .WithColor(Color.Green)
                    .WithTitle($"The sprint has started! It will finish {TimestampTag.FromDateTime(FinishDate.ToUniversalTime(), TimestampTagStyles.Relative)}");
                break;

            case SprintStatus.Stopped:
                embedBuilder
                    .WithColor(Color.Orange)
                    .WithTitle($"The sprint was stopped early.");
                break;

            case SprintStatus.Finished:
                embedBuilder
                    .WithColor(Color.Purple)
                    .WithTitle($"The sprint finished {TimestampTag.FromDateTime(FinishDate.ToUniversalTime(), TimestampTagStyles.LongDateTime)}");
                break;

            default:
                throw new NotImplementedException();
        }

        return embedBuilder.Build();
    }
}
