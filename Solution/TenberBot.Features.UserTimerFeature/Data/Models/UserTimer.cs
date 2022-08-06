using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TenberBot.Features.UserTimerFeature.Data.Enums;

namespace TenberBot.Features.UserTimerFeature.Data.Models;

[Table("UserTimers")]
[Index(nameof(UserTimerStatus))]
public class UserTimer
{
    [Key]
    public int UserTimerId { get; set; }

    public ulong ChannelId { get; set; }

    public ulong UserId { get; set; }

    public UserTimerStatus UserTimerStatus { get; set; }

    public string? Detail { get; set; }

    public DateTime Duration { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime FinishDate { get; set; }

    public UserTimerStatus? GetNextStatus()
    {
        if (UserTimerStatus == UserTimerStatus.Stopped || UserTimerStatus == UserTimerStatus.Finished)
            return null;

        if (DateTime.Now > FinishDate)
            return UserTimerStatus.Finished;

        return null;
    }
}
