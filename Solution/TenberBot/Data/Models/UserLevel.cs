using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

    [Precision(20, 0)]
    public decimal VoiceMinutes { get; set; }

    [Precision(20, 0)]
    public decimal VoiceMinutesStream { get; set; }

    [Precision(20, 0)]
    public decimal VoiceMinutesVideo { get; set; }

    [Precision(20, 0)]
    public decimal ExcludedVoiceMinutes { get; set; }

    [Precision(20, 0)]
    public decimal ExcludedVoiceMinutesVideo { get; set; }

    [Precision(20, 0)]
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


    public void AddMessage(ExperienceChannelSettings settings, int attachments, int lines, int words, int characters)
    {
        var experience = 0m;

        if (settings.Enabled && settings.Message > 0)
        {
            Messages += 1;
            experience += settings.Message;
        }
        else
            ExcludedMessages += 1;

        if (lines > 0)
        {
            if (settings.Enabled && settings.MessageLine > 0)
            {
                MessageLines += lines;
                experience += settings.MessageLine * lines;
            }
            else
                ExcludedMessageLines += lines;

            if (settings.Enabled && settings.MessageWord > 0)
            {
                MessageWords += words;
                experience += settings.MessageWord * words;
            }
            else
                ExcludedMessageWords += words;

            if (settings.Enabled && settings.MessageCharacter > 0)
            {
                MessageCharacters += characters;
                experience += settings.MessageCharacter * characters;
            }
            else
                ExcludedMessageCharacters += characters;
        }

        if (attachments > 0)
        {
            if (settings.Enabled && settings.MessageAttachment > 0)
            {
                MessageAttachments += attachments;
                experience += settings.MessageAttachment * attachments;
            }
            else
                ExcludedMessageLines += attachments;
        }

#if DEBUG
        Console.WriteLine($"{UserId} MessageExperience gain: {experience}");
#endif

        MessageExperience += experience;

        MessageLevel = CalculateLevel(MessageExperience);
    }

    public void AddVoice(ExperienceChannelSettings settings, int minutes, int minutesVideo, int minutesStream)
    {
        var experience = 0m;

        if (settings.Enabled && settings.VoiceMinute > 0)
        {
            VoiceMinutes += minutes;
            experience += settings.VoiceMinute * minutes;
        }
        else
            ExcludedVoiceMinutes += minutes;

        if (minutesVideo > 0)
        {
            if (settings.Enabled && settings.VoiceMinuteVideo > 0)
            {
                VoiceMinutesVideo += minutesVideo;
                experience += settings.VoiceMinuteVideo * minutesVideo;
            }
            else
                ExcludedVoiceMinutesVideo += minutesVideo;
        }

        if (minutesStream > 0)
        {
            if (settings.Enabled && settings.VoiceMinuteStream > 0)
            {
                VoiceMinutesStream += minutesStream;
                experience += settings.VoiceMinuteStream * minutesStream;
            }
            else
                ExcludedVoiceMinutesStream += minutesStream;
        }

#if DEBUG
        Console.WriteLine($"{UserId} VoiceExperience gain: {experience}");
#endif

        VoiceExperience += experience;

        VoiceLevel = CalculateLevel(VoiceExperience);
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
