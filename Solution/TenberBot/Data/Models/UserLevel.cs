using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TenberBot.Data.Enums;
using TenberBot.Data.Settings.Channel;

namespace TenberBot.Data.Models;

[Table("UserLevels")]
[Index(nameof(GuildId), nameof(UserId), IsUnique = true)]
public class UserLevel
{
    [Key]
    public int UserLevelId { get; set; }

    public ulong GuildId { get; set; }

    public ulong UserId { get; set; }

    public int VoiceLevel { get; set; } = 1;

    [Precision(20, 2)]
    public decimal VoiceExperience { get; set; }

    [Precision(20, 2)]
    public decimal VoiceMinutes { get; set; }

    [Precision(20, 2)]
    public decimal VoiceMinutesStream { get; set; }

    [Precision(20, 2)]
    public decimal VoiceMinutesVideo { get; set; }

    [Precision(20, 2)]
    public decimal ExcludedVoiceMinutes { get; set; }

    [Precision(20, 2)]
    public decimal ExcludedVoiceMinutesVideo { get; set; }

    [Precision(20, 2)]
    public decimal ExcludedVoiceMinutesStream { get; set; }

    public int MessageLevel { get; set; } = 1;

    [Precision(20, 2)]
    public decimal MessageExperience { get; set; }

    [Precision(20, 0)]
    public decimal Messages { get; set; }

    [Precision(20, 0)]
    public decimal MessageLines { get; set; }

    [Precision(20, 0)]
    public decimal MessageWords { get; set; }

    [Precision(20, 0)]
    public decimal MessageCharacters { get; set; }

    [Precision(20, 0)]
    public decimal MessageAttachments { get; set; }

    [Precision(20, 0)]
    public decimal ExcludedMessages { get; set; }

    [Precision(20, 0)]
    public decimal ExcludedMessageLines { get; set; }

    [Precision(20, 0)]
    public decimal ExcludedMessageWords { get; set; }

    [Precision(20, 0)]
    public decimal ExcludedMessageCharacters { get; set; }

    [Precision(20, 0)]
    public decimal ExcludedMessageAttachments { get; set; }

    [Precision(20, 0)]
    public decimal EventExperience { get; set; }

    [ForeignKey("GuildId,UserId")]
    public ServerUser ServerUser { get; set; } = null!;

    public decimal VoiceExperienceTotalCurrentLevel => CalculateExperience(VoiceLevel - 1);
    public decimal VoiceExperienceTotalNextLevel => CalculateExperience(VoiceLevel);
    public decimal VoiceExperienceAmountCurrentLevel => VoiceExperience - VoiceExperienceTotalCurrentLevel;
    public decimal VoiceExperienceRequiredCurrentLevel => VoiceExperienceTotalNextLevel - VoiceExperienceTotalCurrentLevel;

    public decimal MessageExperienceTotalCurrentLevel => CalculateExperience(MessageLevel - 1);
    public decimal MessageExperienceTotalNextLevel => CalculateExperience(MessageLevel);
    public decimal MessageExperienceAmountCurrentLevel => MessageExperience - MessageExperienceTotalCurrentLevel;
    public decimal MessageExperienceRequiredCurrentLevel => MessageExperienceTotalNextLevel - MessageExperienceTotalCurrentLevel;


    [NotMapped]
    public int VoiceRank { get; set; }

    [NotMapped]
    public int MessageRank { get; set; }

    [NotMapped]
    public int EventRank { get; set; }

    public (int, decimal) GetLeaderboardData(LeaderboardType leaderboardType)
    {
        return leaderboardType switch
        {
            LeaderboardType.Message => (MessageLevel, MessageExperience),
            LeaderboardType.Voice => (VoiceLevel, VoiceExperience),
            LeaderboardType.Event => (-1, EventExperience),
            _ => throw new NotImplementedException(),
        };
    }

    public void UpdateMessageLevel()
    {
        MessageLevel = CalculateLevel(MessageExperience);
    }

    public void UpdateVoiceLevel()
    {
        VoiceLevel = CalculateLevel(VoiceExperience);
    }

    public void AddMessage(ExperienceChannelSettings settings, int attachments, int lines, int words, int characters)
    {
        var experience = 0m;
        var enabled = settings.Mode != ExperienceMode.Disabled;

        if (enabled && settings.Message > 0)
        {
            Messages += 1;
            experience += settings.Message;
        }
        else
            ExcludedMessages += 1;

        if (lines > 0)
        {
            if (enabled && settings.MessageLine > 0)
            {
                MessageLines += lines;
                experience += settings.MessageLine * lines;
            }
            else
                ExcludedMessageLines += lines;

            if (enabled && settings.MessageWord > 0)
            {
                MessageWords += words;
                experience += settings.MessageWord * words;
            }
            else
                ExcludedMessageWords += words;

            if (enabled && settings.MessageCharacter > 0)
            {
                MessageCharacters += characters;
                experience += settings.MessageCharacter * characters;
            }
            else
                ExcludedMessageCharacters += characters;
        }

        if (attachments > 0)
        {
            if (enabled && settings.MessageAttachment > 0)
            {
                MessageAttachments += attachments;
                experience += settings.MessageAttachment * attachments;
            }
            else
                ExcludedMessageLines += attachments;
        }

#if DEBUG
        Console.WriteLine($"{GuildId} {UserId} AddMessage: {experience}");
#endif

        switch (settings.Mode)
        {
            case ExperienceMode.Disabled:
                break;

            case ExperienceMode.Normal:
                MessageExperience += experience;
                UpdateMessageLevel();
                break;

            case ExperienceMode.Event:
                EventExperience += experience;
                break;
        }
    }

    public void AddVoice(ExperienceChannelSettings settings, decimal minutes, decimal minutesVideo, decimal minutesStream)
    {
        var experience = 0m;
        var enabled = settings.Mode != ExperienceMode.Disabled;

        if (enabled && settings.VoiceMinute > 0)
        {
            VoiceMinutes += minutes;
            experience += settings.VoiceMinute * minutes;
        }
        else
            ExcludedVoiceMinutes += minutes;

        if (minutesVideo > 0)
        {
            if (enabled && settings.VoiceMinuteVideo > 0)
            {
                VoiceMinutesVideo += minutesVideo;
                experience += settings.VoiceMinuteVideo * minutesVideo;
            }
            else
                ExcludedVoiceMinutesVideo += minutesVideo;
        }

        if (minutesStream > 0)
        {
            if (enabled && settings.VoiceMinuteStream > 0)
            {
                VoiceMinutesStream += minutesStream;
                experience += settings.VoiceMinuteStream * minutesStream;
            }
            else
                ExcludedVoiceMinutesStream += minutesStream;
        }

#if DEBUG
        Console.WriteLine($"{GuildId} {UserId} AddVoice: {experience}");
#endif

        switch (settings.Mode)
        {
            case ExperienceMode.Disabled:
                break;

            case ExperienceMode.Normal:
                VoiceExperience += experience;
                UpdateVoiceLevel();
                break;

            case ExperienceMode.Event:
                EventExperience += experience;
                break;
        }
    }

    private static int CalculateLevel(decimal experience)
    {
        // largest Triangular Number less than Experience by factor of 100
        // https://gamedev.stackexchange.com/a/13639
        // https://en.wikipedia.org/wiki/Triangular_number
        // https://math.stackexchange.com/questions/1417579/largest-triangular-number-less-than-a-given-natural-number

        return (int)((-1 + Math.Sqrt(8 * (decimal.ToDouble(experience) / 100) + 1)) / 2) + 1;
    }

    private static decimal CalculateExperience(int level)
    {
        return ((level * (level + 1)) / 2) * 100;
    }
}
