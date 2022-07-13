using Discord.WebSocket;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System.Reflection;
using TenberBot.Data.Models;
using TenberBot.Data.Settings.Server;
using Color = SixLabors.ImageSharp.Color;

namespace TenberBot.Extensions.ImageSharp;

public static class RankCardImageSharpExtensions
{
    public static IImageProcessingContext AddRankData(
        this IImageProcessingContext processingContext,
        RankCardSettings card,
        SocketGuild guild,
        SocketUser user,
        UserLevel userLevel)
    {
        var assembly = Assembly.GetEntryAssembly()!;
        var fontCollection = new FontCollection();

        var segoeui = fontCollection.Add(assembly.GetManifestResourceStream("TenberBot.Fonts.segoeui.ttf")!);
        var segoeuii = fontCollection.Add(assembly.GetManifestResourceStream("TenberBot.Fonts.segoeuii.ttf")!);
        var segoeuib = fontCollection.Add(assembly.GetManifestResourceStream("TenberBot.Fonts.segoeuib.ttf")!);
        var seguiemj = fontCollection.Add(assembly.GetManifestResourceStream("TenberBot.Fonts.seguiemj.ttf")!);
        var seguihis = fontCollection.Add(assembly.GetManifestResourceStream("TenberBot.Fonts.seguihis.ttf")!);

        var fallbackFontFamilies = new List<FontFamily>() { seguiemj, seguihis };

        var font24 = segoeui.CreateFont(24);
        var font28 = segoeui.CreateFont(28);
        var font28b = segoeui.CreateFont(28, FontStyle.Bold);
        var font24i = segoeuii.CreateFont(24, FontStyle.Italic);
        var font32b = segoeuib.CreateFont(32, FontStyle.Bold);

        //return processingContext
        //    // Guild Name
        //    .DrawText(
        //        new TextOptions(font24i)
        //        {
        //            Origin = new PointF(795, 0),
        //            HorizontalAlignment = HorizontalAlignment.Right,
        //            FallbackFontFamilies = fallbackFontFamilies,
        //        },
        //        "WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW",
        //        Color.White
        //    )
        //    // User Name
        //    .DrawText(
        //        new TextOptions(font32b)
        //        {
        //            Origin = new PointF(210, 40),
        //            FallbackFontFamilies = fallbackFontFamilies,
        //        },
        //        "WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW",
        //        Color.White
        //    )
        //    // Message Rank
        //    .DrawText(
        //        new TextOptions(font28b)
        //        {
        //            Origin = new PointF(318, 132),
        //            HorizontalAlignment = HorizontalAlignment.Center,
        //        },
        //        "000",
        //        Color.White
        //    )
        //    // Message Level
        //    .DrawText(
        //        "000",
        //        font28,
        //        Color.White,
        //        new PointF(425, 96)
        //    )
        //    // Message Total Experience
        //    .DrawText(
        //        new TextOptions(font28)
        //        {
        //            Origin = new PointF(780, 96),
        //            HorizontalAlignment = HorizontalAlignment.Right,
        //        },
        //        $"000,000,000 exp",
        //        Color.White
        //    )
        //    // Message fill
        //    .Fill(
        //        Color.DarkRed,
        //        new RectangleF(364, 138, 414, 30)
        //    )
        //    // Message Current Experience
        //    .DrawText(
        //        new TextOptions(font24)
        //        {
        //            Origin = new PointF(571, 134),
        //            HorizontalAlignment = HorizontalAlignment.Center,
        //        },
        //        "0,000,000 / 0,000,000",
        //        Color.White
        //    )
        //    // Voice Rank
        //    .DrawText(
        //        new TextOptions(font28b)
        //        {
        //            Origin = new PointF(318, 218),
        //            HorizontalAlignment = HorizontalAlignment.Center,
        //        },
        //        "000",
        //        Color.White
        //    )
        //    // Voice Level
        //    .DrawText(
        //        "000",
        //        font28,
        //        Color.White,
        //        new PointF(425, 182)
        //    )
        //    // Voice Total Experience
        //    .DrawText(
        //        new TextOptions(font28)
        //        {
        //            Origin = new PointF(780, 182),
        //            HorizontalAlignment = HorizontalAlignment.Right,
        //        },
        //        $"000,000,000 exp",
        //        Color.White
        //    )
        //    // Voice fill
        //    .Fill(
        //        Color.DarkRed,
        //        new RectangleF(364, 224, 414, 30)
        //    )
        //    // Voice Current Experience
        //    .DrawText(
        //        new TextOptions(font24)
        //        {
        //            Origin = new PointF(571, 220),
        //            HorizontalAlignment = HorizontalAlignment.Center,
        //        },
        //        "0,000,000 / 0,000,000",
        //        Color.White
        //    );

        return processingContext
            // Guild Name
            .DrawText(
                new TextOptions(font24i)
                {
                    Origin = new PointF(795, 0),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    FallbackFontFamilies = fallbackFontFamilies,
                },
                guild.Name,
                Color.ParseHex(card.GuildColor)
            )
            // User Name
            .DrawText(
                new TextOptions(font32b)
                {
                    Origin = new PointF(210, 40),
                    FallbackFontFamilies = fallbackFontFamilies,
                },
                user.GetDisplayName(),
                Color.ParseHex(card.UserColor)
            )
            // Message Rank
            .DrawText(
                new TextOptions(font28b)
                {
                    Origin = new PointF(318, 132),
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
                userLevel.MessageRank.ToString(),
                Color.ParseHex(card.RankColor)
            )
            // Message Level
            .DrawText(
                userLevel.MessageLevel.ToString(),
                font28,
                Color.ParseHex(card.LevelColor),
                new PointF(425, 96)
            )
            // Message Total Experience
            .DrawText(
                new TextOptions(font28)
                {
                    Origin = new PointF(780, 96),
                    HorizontalAlignment = HorizontalAlignment.Right,
                },
                $"{userLevel.MessageExperience:N2} exp",
                Color.ParseHex(card.ExperienceColor)
            )
            // Message fill
            .Fill(
                Color.ParseHex(card.ProgressFill),
                new RectangleF(364, 138, 414 * (float)(userLevel.MessageExperienceAmountCurrentLevel / userLevel.MessageExperienceRequiredCurrentLevel), 30)
            )
            // Message Current Experience
            .DrawText(
                new TextOptions(font24)
                {
                    Origin = new PointF(571, 134),
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
                $"{userLevel.MessageExperienceAmountCurrentLevel:N2} / {userLevel.MessageExperienceRequiredCurrentLevel:N0}",
                Color.ParseHex(card.ProgressColor)
            )
            // Voice Rank
            .DrawText(
                new TextOptions(font28b)
                {
                    Origin = new PointF(318, 218),
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
                userLevel.VoiceRank.ToString(),
                Color.ParseHex(card.RankColor)
            )
            // Voice Level
            .DrawText(
                userLevel.VoiceLevel.ToString(),
                font28,
                Color.ParseHex(card.LevelColor),
                new PointF(425, 182)
            )
            // Voice Total Experience
            .DrawText(
                new TextOptions(font28)
                {
                    Origin = new PointF(780, 182),
                    HorizontalAlignment = HorizontalAlignment.Right,
                },
                $"{userLevel.VoiceExperience:N2} exp",
                Color.ParseHex(card.ExperienceColor)
            )
            // Voice fill
            .Fill(
                Color.ParseHex(card.ProgressFill),
                new RectangleF(364, 224, 414 * (float)(userLevel.VoiceExperienceAmountCurrentLevel / userLevel.VoiceExperienceRequiredCurrentLevel), 30)
            )
            // Voice Current Experience
            .DrawText(
                new TextOptions(font24)
                {
                    Origin = new PointF(571, 220),
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
                $"{userLevel.VoiceExperienceAmountCurrentLevel:N2} / {userLevel.VoiceExperienceRequiredCurrentLevel:N0}",
                Color.ParseHex(card.ProgressColor)
            );
    }
}
