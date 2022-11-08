using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TenberBot.Features.ExperienceFeature.Data.Enums;
using TenberBot.Features.ExperienceFeature.Data.POCO;
using TenberBot.Features.ExperienceFeature.Settings.Channel;

namespace TenberBot.Features.ExperienceFeature.Data.Models;

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
    public decimal EventAExperience { get; set; }

    [Precision(20, 0)]
    public decimal EventBExperience { get; set; }

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
            LeaderboardType.EventA => (-1, EventAExperience),
            LeaderboardType.EventB => (-1, EventBExperience),
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

    public void AddStats(bool enabled, IExperienceModeChannelSettings settings, MessageStats stats)
    {
        var experience = 0m;

        if (enabled && settings.Message > 0)
        {
            Messages += 1;
            experience += settings.Message;
        }
        else
            ExcludedMessages += 1;

        if (stats.Lines > 0)
        {
            if (enabled && settings.MessageLine > 0)
            {
                MessageLines += stats.Lines;
                experience += settings.MessageLine * stats.Lines;
            }
            else
                ExcludedMessageLines += stats.Lines;

            if (enabled && settings.MessageWord > 0)
            {
                MessageWords += stats.Words;
                experience += settings.MessageWord * stats.Words;
            }
            else
                ExcludedMessageWords += stats.Words;

            if (enabled && settings.MessageCharacter > 0)
            {
                MessageCharacters += stats.Characters;
                experience += settings.MessageCharacter * stats.Characters;
            }
            else
                ExcludedMessageCharacters += stats.Characters;
        }

        if (stats.Attachments > 0)
        {
            if (enabled && settings.MessageAttachment > 0)
            {
                MessageAttachments += stats.Attachments;
                experience += settings.MessageAttachment * stats.Attachments;
            }
            else
                ExcludedMessageLines += stats.Attachments;
        }

#if DEBUG
        Console.WriteLine($"{GuildId} {UserId} AddStats NormalMessage experience:{experience} enabled:{enabled}");
#endif

        MessageExperience += experience;
        UpdateMessageLevel();
    }

    public void AddStats(string eventName, IExperienceModeChannelSettings settings, MessageStats stats)
    {
        var experience = 0m;

        if (settings.Message > 0)
            experience += settings.Message;

        if (stats.Lines > 0)
        {
            if (settings.MessageLine > 0)
                experience += settings.MessageLine * stats.Lines;

            if (settings.MessageWord > 0)
                experience += settings.MessageWord * stats.Words;

            if (settings.MessageCharacter > 0)
                experience += settings.MessageCharacter * stats.Characters;
        }

        if (stats.Attachments > 0)
        {
            if (settings.MessageAttachment > 0)
                experience += settings.MessageAttachment * stats.Attachments;
        }

#if DEBUG
        Console.WriteLine($"{GuildId} {UserId} AddStats {eventName}Message experience:{experience}");
#endif

        if (eventName == "EventA")
            EventAExperience += experience;
        else
            EventBExperience += experience;
    }

    public void AddStats(bool enabled, IExperienceModeChannelSettings settings, VoiceStats stats)
    {
        var experience = 0m;

        if (enabled && settings.VoiceMinute > 0)
        {
            VoiceMinutes += stats.Minutes;
            experience += settings.VoiceMinute * stats.Minutes;
        }
        else
            ExcludedVoiceMinutes += stats.Minutes;

        if (stats.MinutesVideo > 0)
        {
            if (enabled && settings.VoiceMinuteVideo > 0)
            {
                VoiceMinutesVideo += stats.MinutesVideo;
                experience += settings.VoiceMinuteVideo * stats.MinutesVideo;
            }
            else
                ExcludedVoiceMinutesVideo += stats.MinutesVideo;
        }

        if (stats.MinutesStream > 0)
        {
            if (enabled && settings.VoiceMinuteStream > 0)
            {
                VoiceMinutesStream += stats.MinutesStream;
                experience += settings.VoiceMinuteStream * stats.MinutesStream;
            }
            else
                ExcludedVoiceMinutesStream += stats.MinutesStream;
        }

#if DEBUG
        Console.WriteLine($"{GuildId} {UserId} AddStats NormalVoice experience:{experience} enabled:{enabled}");
#endif

        VoiceExperience += experience;
        UpdateVoiceLevel();
    }

    public void AddStats(string eventName, IExperienceModeChannelSettings settings, VoiceStats stats)
    {
        var experience = 0m;

        if (settings.VoiceMinute > 0)
            experience += settings.VoiceMinute * stats.Minutes;

        if (stats.MinutesVideo > 0)
        {
            if (settings.VoiceMinuteVideo > 0)
                experience += settings.VoiceMinuteVideo * stats.MinutesVideo;
        }

        if (stats.MinutesStream > 0)
        {
            if (settings.VoiceMinuteStream > 0)
                experience += settings.VoiceMinuteStream * stats.MinutesStream;
        }

#if DEBUG
        Console.WriteLine($"{GuildId} {UserId} AddStats {eventName}Voice experience:{experience}");
#endif

        if (eventName == "EventA")
            EventAExperience += experience;
        else
            EventBExperience += experience;
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
