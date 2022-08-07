using Discord;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TenberBot.Features.MessageTimerFeature.Data.Enums;

namespace TenberBot.Features.MessageTimerFeature.Data.Models;

[Table("MessageTimers")]
[Index(nameof(MessageTimerStatus))]
public class MessageTimer
{
    [Key]
    public int MessageTimerId { get; set; }

    public ulong ChannelId { get; set; }

    public ulong UserId { get; set; }

    public MessageTimerStatus MessageTimerStatus { get; set; }

    public ulong TargetChannelId { get; set; }

    public string Detail { get; set; } = "\u200B";

    public string? Filename { get; set; }

    public byte[]? Data { get; set; }

    public DateTime Duration { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime FinishDate { get; set; }

    public FileAttachment AsAttachment() => new(new MemoryStream(Data ?? Array.Empty<byte>()), $"{MessageTimerId}_{Filename}");

    public MessageTimerStatus? GetNextStatus()
    {
        if (MessageTimerStatus == MessageTimerStatus.Stopped || MessageTimerStatus == MessageTimerStatus.Finished)
            return null;

        if (DateTime.Now > FinishDate)
            return MessageTimerStatus.Finished;

        return null;
    }
}
